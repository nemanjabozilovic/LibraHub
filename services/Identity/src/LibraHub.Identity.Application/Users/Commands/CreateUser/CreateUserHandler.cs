using LibraHub.BuildingBlocks.Abstractions;
using LibraHub.BuildingBlocks.Results;
using LibraHub.Identity.Application.Abstractions;
using LibraHub.Identity.Application.Constants;
using LibraHub.Identity.Domain.Tokens;
using LibraHub.Identity.Domain.Users;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Error = LibraHub.BuildingBlocks.Results.Error;

namespace LibraHub.Identity.Application.Users.Commands.CreateUser;

public class CreateUserHandler(
    IUserRepository userRepository,
    IRegistrationCompletionTokenRepository tokenRepository,
    IRegistrationCompletionTokenService tokenService,
    IPasswordHasher passwordHasher,
    IEmailSender emailSender,
    IConfiguration configuration,
    IUnitOfWork unitOfWork,
    ILogger<CreateUserHandler> logger) : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var emailLower = request.Email.ToLowerInvariant();

        if (await userRepository.ExistsByEmailAsync(emailLower, cancellationToken))
        {
            return Result.Failure<Guid>(Error.Conflict("Email already exists"));
        }

        var tempPassword = GenerateTemporaryPassword();
        var passwordHash = passwordHasher.HashPassword(tempPassword);

        var user = new User(
            Guid.NewGuid(),
            emailLower,
            passwordHash,
            string.Empty,
            string.Empty,
            null,
            null);

        user.RemoveRole(Role.User);
        user.AddRole(request.Role);

        string completionToken;

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            await userRepository.AddAsync(user, cancellationToken);

            completionToken = tokenService.GenerateToken();
            var tokenExpiration = tokenService.GetExpiration();
            var registrationToken = new RegistrationCompletionToken(
                Guid.NewGuid(),
                user.Id,
                completionToken,
                tokenExpiration);

            await tokenRepository.AddAsync(registrationToken, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Failed to create user with email: {Email}", emailLower);
            throw;
        }

        var frontendUrl = configuration["Frontend:BaseUrl"]
            ?? throw new InvalidOperationException("Frontend:BaseUrl configuration is required");
        var completionLink = $"{frontendUrl}/complete-registration?token={completionToken}";
        var emailSubject = EmailMessages.CompleteRegistration;
        var emailModel = new
        {
            Email = user.Email,
            CompletionLink = completionLink,
            ExpirationHours = tokenService.GetExpirationHours()
        };

        try
        {
            await emailSender.SendEmailWithTemplateAsync(
                user.Email,
                emailSubject,
                "COMPLETE_REGISTRATION",
                emailModel,
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send registration completion email to {Email}", user.Email);
        }

        return Result.Success(user.Id);
    }

    private static string GenerateTemporaryPassword()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 16)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}

