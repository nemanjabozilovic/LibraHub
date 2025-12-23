using LibraHub.BuildingBlocks.Abstractions;
using LibraHub.BuildingBlocks.Results;
using LibraHub.Contracts.Common;
using LibraHub.Contracts.Identity.V1;
using LibraHub.Identity.Application.Abstractions;
using LibraHub.Identity.Application.Constants;
using LibraHub.Identity.Domain.Users;
using MediatR;
using Microsoft.Extensions.Logging;
using Error = LibraHub.BuildingBlocks.Results.Error;

namespace LibraHub.Identity.Application.Auth.Commands.Register;

public class RegisterHandler(
    IUserRepository userRepository,
    IEmailVerificationTokenRepository tokenRepository,
    IPasswordHasher passwordHasher,
    IEmailVerificationTokenService tokenService,
    IOutboxWriter outboxWriter,
    IEmailSender emailSender,
    IClock clock,
    IUnitOfWork unitOfWork,
    ILogger<RegisterHandler> logger) : IRequestHandler<RegisterCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var emailLower = request.Email.ToLowerInvariant();

        if (await userRepository.ExistsByEmailAsync(emailLower, cancellationToken))
        {
            return Result.Failure<Guid>(Error.Conflict("Email already exists"));
        }

        var passwordHash = passwordHasher.HashPassword(request.Password);
        var user = new User(
            Guid.NewGuid(),
            emailLower,
            passwordHash,
            request.FirstName ?? string.Empty,
            request.LastName ?? string.Empty,
            request.Phone,
            request.DateOfBirth);

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            await userRepository.AddAsync(user, cancellationToken);

            var verificationToken = tokenService.GenerateToken();
            var tokenExpiration = tokenService.GetExpiration();
            var emailVerificationToken = new Domain.Tokens.EmailVerificationToken(
                Guid.NewGuid(),
                user.Id,
                verificationToken,
                tokenExpiration);

            await tokenRepository.AddAsync(emailVerificationToken, cancellationToken);

            var integrationEvent = new UserRegisteredV1
            {
                UserId = user.Id,
                Email = user.Email,
                OccurredAt = clock.UtcNow
            };

            await outboxWriter.WriteAsync(integrationEvent, EventTypes.UserRegistered, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Failed to register user with email: {Email}", emailLower);
            throw;
        }

        var emailSubject = EmailMessages.Welcome;
        var emailModel = new
        {
            FullName = !string.IsNullOrWhiteSpace(user.DisplayName) ? user.DisplayName : user.Email,
            TempPassword = request.Password
        };

        try
        {
            await emailSender.SendEmailWithTemplateAsync(
                user.Email,
                emailSubject,
                "REGISTER",
                emailModel,
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send welcome email to {Email}", user.Email);
        }

        return Result.Success(user.Id);
    }
}
