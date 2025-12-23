using LibraHub.Library.Domain.Entitlements;

namespace LibraHub.Library.Application.Abstractions;

public interface IEntitlementRepository
{
    Task<Entitlement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Entitlement?> GetByUserAndBookAsync(Guid userId, Guid bookId, CancellationToken cancellationToken = default);

    Task<List<Entitlement>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<List<Entitlement>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<bool> HasAccessAsync(Guid userId, Guid bookId, CancellationToken cancellationToken = default);

    Task AddAsync(Entitlement entitlement, CancellationToken cancellationToken = default);

    Task UpdateAsync(Entitlement entitlement, CancellationToken cancellationToken = default);
}

