using LibraHub.BuildingBlocks.Abstractions;
using LibraHub.Contracts.Common;
using LibraHub.Contracts.Library.V1;
using LibraHub.Contracts.Orders.V1;
using LibraHub.Library.Application.Abstractions;
using LibraHub.Library.Domain.Entitlements;
using Microsoft.Extensions.Logging;

namespace LibraHub.Library.Application.Consumers;

public class OrderPaidConsumer(
    IEntitlementRepository entitlementRepository,
    BuildingBlocks.Abstractions.IOutboxWriter outboxWriter,
    IUnitOfWork unitOfWork,
    ILogger<OrderPaidConsumer> logger)
{
    public async Task HandleAsync(OrderPaidV1 @event, CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing OrderPaid event for OrderId: {OrderId}, UserId: {UserId}", @event.OrderId, @event.UserId);

        try
        {
            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                foreach (var item in @event.Items)
                {
                    var existing = await entitlementRepository.GetByUserAndBookAsync(
                        @event.UserId,
                        item.BookId,
                        ct);

                    if (existing != null)
                    {
                        if (existing.IsActive)
                        {
                            logger.LogInformation("Entitlement already exists and is active for UserId: {UserId}, BookId: {BookId}",
                                @event.UserId, item.BookId);
                            continue;
                        }

                        existing.Reactivate();
                        await entitlementRepository.UpdateAsync(existing, ct);

                        logger.LogInformation("Reactivated entitlement for UserId: {UserId}, BookId: {BookId}",
                            @event.UserId, item.BookId);
                    }
                    else
                    {
                        var entitlement = new Entitlement(
                            Guid.NewGuid(),
                            @event.UserId,
                            item.BookId,
                            EntitlementSource.Purchase,
                            @event.OrderId);

                        await entitlementRepository.AddAsync(entitlement, ct);

                        logger.LogInformation("Created entitlement for UserId: {UserId}, BookId: {BookId}",
                            @event.UserId, item.BookId);
                    }

                    await outboxWriter.WriteAsync(
                        new EntitlementGrantedV1
                        {
                            UserId = @event.UserId,
                            BookId = item.BookId,
                            Source = EntitlementSource.Purchase.ToString(),
                            AcquiredAtUtc = @event.PaidAt
                        },
                        EventTypes.EntitlementGranted,
                        ct);
                }
            }, cancellationToken);

            logger.LogInformation("Completed processing OrderPaid event for OrderId: {OrderId}", @event.OrderId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process OrderPaid event for OrderId: {OrderId}", @event.OrderId);
            throw;
        }
    }
}

