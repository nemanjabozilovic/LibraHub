using LibraHub.BuildingBlocks.Abstractions;
using LibraHub.BuildingBlocks.Results;
using LibraHub.Identity.Application.Abstractions;
using LibraHub.Identity.Application.Constants;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LibraHub.Identity.Application.Auth.Commands.ForgotPassword;

public class ForgotPasswordHandler(
    IUserRepository userRepository,
    IPasswordResetTokenRepository tokenRepository,
    IPasswordResetTokenService tokenService,
    IEmailSender emailSender,
    IConfiguration configuration,
    ILogger<ForgotPasswordHandler> logger) : IRequestHandler<ForgotPasswordCommand, Result>
{
    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var emailLower = request.Email.ToLowerInvariant();
        var user = await userRepository.GetByEmailAsync(emailLower, cancellationToken);

        if (user == null)
        {
            logger.LogWarning("Password reset requested for non-existent email: {Email}", emailLower);
            return Result.Success();
        }

        var resetToken = tokenService.GenerateToken();
        var tokenExpiration = tokenService.GetExpiration();
        var passwordResetToken = new Domain.Tokens.PasswordResetToken(
            Guid.NewGuid(),
            user.Id,
            resetToken,
            tokenExpiration);

        await tokenRepository.AddAsync(passwordResetToken, cancellationToken);

        var frontendUrl = configuration["Frontend:BaseUrl"] ?? "http://localhost:3000";
        var resetLink = $"{frontendUrl}/reset-password?token={resetToken}";

        var emailSubject = EmailMessages.PasswordResetRequest;
        var emailModel = new
        {
            FullName = !string.IsNullOrWhiteSpace(user.Email) ? user.Email.Split('@')[0] : "User",
            ResetLink = resetLink,
            ExpirationHours = 24
        };

        try
        {
            await emailSender.SendEmailWithTemplateAsync(
                user.Email,
                emailSubject,
                "FORGOT_PASSWORD",
                emailModel,
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send password reset email to {Email}", user.Email);
        }

        logger.LogInformation("Password reset token generated for user: {UserId}", user.Id);
        return Result.Success();
    }
}

