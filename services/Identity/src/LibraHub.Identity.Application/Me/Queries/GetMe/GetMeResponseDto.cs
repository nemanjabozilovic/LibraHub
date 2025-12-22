namespace LibraHub.Identity.Application.Me.Queries.GetMe;

public record GetMeResponseDto
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public List<string> Roles { get; init; } = new();
    public bool EmailVerified { get; init; }
    public string Status { get; init; } = string.Empty;
}

