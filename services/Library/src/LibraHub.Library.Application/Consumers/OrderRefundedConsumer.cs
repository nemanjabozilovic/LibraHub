using LibraHub.Contracts.Common;
using LibraHub.Contracts.Library.V1;
using LibraHub.Contracts.Orders.V1;
using LibraHub.Library.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace LibraHub.Library.Application.Consumers;

public class OrderRefundedConsumer(
    IEntitlementRepository entitlementRepository,
    BuildingBlocks.Abstractions.IOutboxWriter outboxWriter,
    ILogger<OrderRefundedConsumer> logger)
{
    public async Task HandleAsync(OrderRefundedV1 @event, CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing OrderRefunded event for OrderId: {OrderId}, UserId: {UserId}",
            @event.OrderId, @event.UserId);

        // Get all entitlements for this user that were created from this order
        var entitlements = await entitlementRepository.GetByUserIdAsync(@event.UserId, cancellationToken);
        var orderEntitlements = entitlements
            .Where(e => e.OrderId == @event.OrderId && e.IsActive)
            .ToList();

        foreach (var entitlement in orderEntitlements)
        {
            entitlement.Revoke(@event.Reason);
            await entitlementRepository.UpdateAsync(entitlement, cancellationToken);

            logger.LogInformation("Revoked entitlement for UserId: {UserId}, BookId: {BookId}",
                @event.UserId, entitlement.BookId);

            // Publish event
            await outboxWriter.WriteAsync(
                new EntitlementRevokedV1
                {
                    UserId = entitlement.UserId,
                    BookId = entitlement.BookId,
                    Reason = @event.Reason,
                    RevokedAtUtc = @event.RefundedAt
                },
                EventTypes.EntitlementRevoked,
                cancellationToken);
        }

        logger.LogInformation("Completed processing OrderRefunded event for OrderId: {OrderId}", @event.OrderId);
    }
}

