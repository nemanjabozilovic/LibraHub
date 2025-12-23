namespace LibraHub.Orders.Application.Abstractions;

public interface IIdentityClient
{
    Task<UserInfo?> GetUserInfoAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}

public class UserInfo
{
    public Guid Id { get; init; }
    public bool IsActive { get; init; }
    public bool IsEmailVerified { get; init; }
}

