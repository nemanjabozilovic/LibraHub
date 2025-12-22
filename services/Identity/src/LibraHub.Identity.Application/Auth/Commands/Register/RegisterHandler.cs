using LibraHub.BuildingBlocks.Abstractions;
using LibraHub.BuildingBlocks.Results;
using LibraHub.Contracts.Common;
using LibraHub.Contracts.Identity.V1;
using LibraHub.Identity.Application.Abstractions;
using LibraHub.Identity.Domain.Users;
using MediatR;
using Error = LibraHub.BuildingBlocks.Results.Error;

namespace LibraHub.Identity.Application.Auth.Commands.Register;

public class RegisterHandler(
    IUserRepository userRepository,
    IEmailVerificationTokenRepository tokenRepository,
    IPasswordHasher passwordHasher,
    IEmailVerificationTokenService tokenService,
    IOutboxWriter outboxWriter,
    IClock clock) : IRequestHandler<RegisterCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var emailLower = request.Email.ToLowerInvariant();

        if (await userRepository.ExistsByEmailAsync(emailLower, cancellationToken))
        {
            return Result.Failure<Guid>(Error.Conflict("Email already exists"));
        }

        var passwordHash = passwordHasher.HashPassword(request.Password);
        var user = new User(Guid.NewGuid(), emailLower, passwordHash);

        await userRepository.AddAsync(user, cancellationToken);

        // Generate email verification token
        var verificationToken = tokenService.GenerateToken();
        var tokenExpiration = tokenService.GetExpiration();
        var emailVerificationToken = new Domain.Tokens.EmailVerificationToken(
            Guid.NewGuid(),
            user.Id,
            verificationToken,
            tokenExpiration);

        await tokenRepository.AddAsync(emailVerificationToken, cancellationToken);

        // Publish integration event
        var integrationEvent = new UserRegisteredV1
        {
            UserId = user.Id,
            Email = user.Email,
            OccurredAt = clock.UtcNow
        };

        await outboxWriter.WriteAsync(integrationEvent, EventTypes.UserRegistered, cancellationToken);

        return Result.Success(user.Id);
    }
}
