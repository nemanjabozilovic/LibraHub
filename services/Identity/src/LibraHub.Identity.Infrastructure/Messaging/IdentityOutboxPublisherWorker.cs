using LibraHub.BuildingBlocks.Outbox;
using LibraHub.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace LibraHub.Identity.Infrastructure.Messaging;

public class IdentityOutboxPublisherWorker(
    IServiceProvider serviceProvider,
    ILogger<IdentityOutboxPublisherWorker> logger,
    IConnection connection) : OutboxPublisherWorker(logger, connection, "librahub.events")
{
    protected override async Task<List<OutboxMessage>> GetPendingMessagesAsync(int batchSize, CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        return await context.OutboxMessages
            .Where(om => om.ProcessedAt == null)
            .OrderBy(om => om.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    protected override async Task MarkAsProcessedAsync(Guid messageId, CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        var message = await context.OutboxMessages.FindAsync([messageId], cancellationToken);
        if (message != null)
        {
            message.ProcessedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    protected override async Task MarkAsFailedAsync(Guid messageId, string error, CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        var message = await context.OutboxMessages.FindAsync(new object[] { messageId }, cancellationToken);
        if (message != null)
        {
            message.Error = error;
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
