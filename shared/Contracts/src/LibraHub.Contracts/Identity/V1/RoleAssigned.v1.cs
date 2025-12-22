namespace LibraHub.Contracts.Identity.V1;

public record RoleAssignedV1
{
    public Guid UserId { get; init; }
    public string Role { get; init; } = string.Empty;
    public DateTime OccurredAt { get; init; }
}
