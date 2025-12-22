using LibraHub.BuildingBlocks.Abstractions;
using LibraHub.BuildingBlocks.Outbox;
using LibraHub.Identity.Infrastructure.Persistence;
using System.Text.Json;

namespace LibraHub.Identity.Infrastructure.Messaging;

public class IdentityEventPublisher : IOutboxWriter
{
    private readonly IdentityDbContext _context;
    private readonly JsonSerializerOptions _jsonOptions;

    public IdentityEventPublisher(IdentityDbContext context)
    {
        _context = context;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task WriteAsync<T>(T integrationEvent, string eventType, CancellationToken cancellationToken = default) where T : class
    {
        var payload = JsonSerializer.Serialize(integrationEvent, _jsonOptions);

        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = eventType,
            Payload = payload,
            CreatedAt = DateTime.UtcNow
        };

        await _context.OutboxMessages.AddAsync(outboxMessage, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
