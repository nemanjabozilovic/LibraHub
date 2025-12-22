namespace LibraHub.Identity.Domain.Users;

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public bool EmailVerified { get; private set; }
    public UserStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockedOutUntil { get; private set; }

    private readonly List<UserRole> _roles = new();
    public IReadOnlyCollection<UserRole> Roles => _roles.AsReadOnly();

    private User()
    { } // For EF Core

    public User(Guid id, string email, string passwordHash)
    {
        Id = id;
        Email = email;
        PasswordHash = passwordHash;
        EmailVerified = false;
        Status = UserStatus.Active;
        CreatedAt = DateTime.UtcNow;
        FailedLoginAttempts = 0;
        _roles.Add(new UserRole(Id, Role.User));
    }

    public void MarkEmailAsVerified()
    {
        EmailVerified = true;
    }

    public void RecordSuccessfulLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        FailedLoginAttempts = 0;
        LockedOutUntil = null;
    }

    public void RecordFailedLogin(int maxAttempts, TimeSpan lockoutDuration)
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= maxAttempts)
        {
            LockedOutUntil = DateTime.UtcNow.Add(lockoutDuration);
        }
    }

    public bool IsLockedOut(DateTime utcNow)
    {
        return LockedOutUntil.HasValue && LockedOutUntil.Value > utcNow;
    }

    public void Disable(string reason)
    {
        Status = UserStatus.Disabled;
    }

    public void Enable()
    {
        Status = UserStatus.Active;
        FailedLoginAttempts = 0;
        LockedOutUntil = null;
    }

    public void AddRole(Role role)
    {
        if (!_roles.Any(r => r.Role == role))
        {
            _roles.Add(new UserRole(Id, role));
        }
    }

    public void RemoveRole(Role role)
    {
        var roleToRemove = _roles.FirstOrDefault(r => r.Role == role);
        if (roleToRemove != null)
        {
            _roles.Remove(roleToRemove);
        }
    }

    public bool HasRole(Role role)
    {
        return _roles.Any(r => r.Role == role);
    }

    public bool IsAdmin()
    {
        return HasRole(Role.Admin);
    }
}

public class UserRole
{
    public Guid UserId { get; private set; }
    public Role Role { get; private set; }
    public DateTime AssignedAt { get; private set; }

    private UserRole()
    { } // For EF Core

    public UserRole(Guid userId, Role role)
    {
        UserId = userId;
        Role = role;
        AssignedAt = DateTime.UtcNow;
    }
}
