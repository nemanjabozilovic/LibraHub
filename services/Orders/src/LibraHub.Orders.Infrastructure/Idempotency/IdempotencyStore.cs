using LibraHub.BuildingBlocks.Idempotency;
using LibraHub.Orders.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LibraHub.Orders.Infrastructure.Idempotency;

public class IdempotencyStore : IIdempotencyStore
{
    private readonly OrdersDbContext _context;

    public IdempotencyStore(OrdersDbContext context)
    {
        _context = context;
    }

    public async Task<IdempotencyResponse?> GetResponseAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        var key = await _context.IdempotencyKeys
            .FirstOrDefaultAsync(k => k.Key == idempotencyKey, cancellationToken);

        if (key == null)
        {
            return null;
        }

        return new IdempotencyResponse
        {
            StatusCode = key.StatusCode,
            ContentType = key.ContentType,
            Body = key.ResponseBody
        };
    }

    public async Task StoreResponseAsync(string idempotencyKey, int statusCode, string contentType, byte[] body, CancellationToken cancellationToken = default)
    {
        var key = new IdempotencyKey
        {
            Key = idempotencyKey,
            StatusCode = statusCode,
            ContentType = contentType,
            ResponseBody = body,
            CreatedAt = DateTime.UtcNow
        };

        _context.IdempotencyKeys.Add(key);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

