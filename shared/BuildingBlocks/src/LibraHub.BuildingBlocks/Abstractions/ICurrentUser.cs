namespace LibraHub.BuildingBlocks.Abstractions;

public interface ICurrentUser
{
    Guid? UserId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }

    bool IsInRole(string role);

    IEnumerable<string> Roles { get; }
}
