namespace LibraHub.Identity.Application.Abstractions;

public interface IEmailVerificationTokenService
{
    string GenerateToken();

    DateTime GetExpiration();
}
