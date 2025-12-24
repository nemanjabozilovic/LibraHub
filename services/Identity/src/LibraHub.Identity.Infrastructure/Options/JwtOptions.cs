using System.ComponentModel.DataAnnotations;

namespace LibraHub.Identity.Infrastructure.Options;

public class JwtOptions
{
    [Required(ErrorMessage = "JWT SecretKey is required")]
    public string SecretKey { get; set; } = string.Empty;

    [Required(ErrorMessage = "JWT Issuer is required")]
    public string Issuer { get; set; } = string.Empty;

    [Required(ErrorMessage = "JWT Audience is required")]
    public string Audience { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "AccessTokenExpirationMinutes must be greater than 0")]
    public int AccessTokenExpirationMinutes { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "RefreshTokenExpirationDays must be greater than 0")]
    public int RefreshTokenExpirationDays { get; set; }
}
