using Microsoft.EntityFrameworkCore;

namespace LibraHub.BuildingBlocks.Abstractions;

public interface IUnitOfWork
{
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IUnitOfWork<TDbContext> : IUnitOfWork where TDbContext : DbContext
{
}

