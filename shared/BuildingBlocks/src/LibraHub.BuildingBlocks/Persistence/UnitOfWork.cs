using LibraHub.BuildingBlocks.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace LibraHub.BuildingBlocks.Persistence;

public class UnitOfWork<TDbContext>(TDbContext context) : IUnitOfWork<TDbContext>
    where TDbContext : DbContext
{
    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        var strategy = context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await context.Database.BeginTransactionAsync(cancellationToken);

            await action(cancellationToken);

            await context.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
        });
    }
}