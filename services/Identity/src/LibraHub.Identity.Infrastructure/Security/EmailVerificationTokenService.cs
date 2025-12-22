using LibraHub.Identity.Application.Abstractions;
using System.Security.Cryptography;

namespace LibraHub.Identity.Infrastructure.Security;

public class EmailVerificationTokenService : IEmailVerificationTokenService
{
    public string GenerateToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public DateTime GetExpiration()
    {
        return DateTime.UtcNow.AddDays(7); // 7 days expiration
    }
}
