using LibraHub.BuildingBlocks.Abstractions;
using LibraHub.BuildingBlocks.Results;
using LibraHub.Orders.Application.Abstractions;
using LibraHub.Orders.Domain.Errors;
using LibraHub.Orders.Domain.Orders;
using MediatR;
using Error = LibraHub.BuildingBlocks.Results.Error;

namespace LibraHub.Orders.Application.Orders.Commands.CreateOrder;

public class CreateOrderHandler(
    IOrderRepository orderRepository,
    ICatalogPricingClient catalogClient,
    ILibraryOwnershipClient libraryClient,
    IIdentityClient identityClient,
    ICurrentUser currentUser,
    IOutboxWriter outboxWriter,
    IClock clock) : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.UserId.HasValue)
        {
            return Result.Failure<Guid>(Error.Unauthorized(OrdersErrors.User.NotAuthenticated));
        }

        var userId = currentUser.UserId.Value;

        // Validate user
        var userInfo = await identityClient.GetUserInfoAsync(userId, cancellationToken);
        if (userInfo == null)
        {
            return Result.Failure<Guid>(Error.NotFound("User not found"));
        }

        if (!userInfo.IsActive)
        {
            return Result.Failure<Guid>(Error.Validation(OrdersErrors.User.NotActive));
        }

        if (!userInfo.IsEmailVerified)
        {
            return Result.Failure<Guid>(Error.Validation(OrdersErrors.User.EmailNotVerified));
        }

        // Get pricing quote from Catalog
        var pricingQuote = await catalogClient.GetPricingQuoteAsync(request.BookIds, userId, cancellationToken);
        if (pricingQuote == null || pricingQuote.Items.Count != request.BookIds.Count)
        {
            return Result.Failure<Guid>(Error.NotFound("Could not retrieve pricing information for all books"));
        }

        // Validate books
        foreach (var item in pricingQuote.Items)
        {
            if (!item.IsPublished)
            {
                return Result.Failure<Guid>(Error.Validation(OrdersErrors.Book.NotPublished));
            }

            if (item.IsRemoved)
            {
                return Result.Failure<Guid>(Error.Validation(OrdersErrors.Book.Removed));
            }

            if (item.FinalPrice == 0)
            {
                return Result.Failure<Guid>(Error.Validation(OrdersErrors.Book.IsFree));
            }
        }

        // Check if user already owns any of the books
        var ownedBookIds = await libraryClient.GetOwnedBookIdsAsync(userId, request.BookIds, cancellationToken);
        if (ownedBookIds.Count > 0)
        {
            return Result.Failure<Guid>(Error.Validation(OrdersErrors.Book.AlreadyOwned));
        }

        // Create order items
        var orderId = Guid.NewGuid();
        var orderItems = new List<OrderItem>();
        var currency = pricingQuote.Currency;
        var subtotal = Money.Zero(currency);
        var vatTotal = Money.Zero(currency);

        foreach (var quoteItem in pricingQuote.Items)
        {
            var basePrice = new Money(quoteItem.BasePrice, currency);
            var finalPrice = new Money(quoteItem.FinalPrice, currency);
            var vatAmount = new Money(quoteItem.FinalPrice * (quoteItem.VatRate / 100m), currency);

            var orderItem = new OrderItem(
                Guid.NewGuid(),
                orderId,
                quoteItem.BookId,
                quoteItem.BookTitle,
                basePrice,
                finalPrice,
                quoteItem.VatRate,
                vatAmount,
                quoteItem.PromotionId,
                quoteItem.PromotionName,
                quoteItem.DiscountAmount);

            orderItems.Add(orderItem);
            subtotal = subtotal.Add(finalPrice);
            vatTotal = vatTotal.Add(vatAmount);
        }

        // Create order
        var order = new Order(
            orderId,
            userId,
            orderItems,
            subtotal,
            vatTotal,
            subtotal.Add(vatTotal));

        await orderRepository.AddAsync(order, cancellationToken);

        // Publish event
        await outboxWriter.WriteAsync(
            new Contracts.Orders.V1.OrderCreatedV1
            {
                OrderId = order.Id,
                UserId = order.UserId,
                Items = order.Items.Select(i => new Contracts.Orders.V1.OrderItemDto
                {
                    BookId = i.BookId,
                    BookTitle = i.BookTitle,
                    BasePrice = i.BasePrice.Amount,
                    FinalPrice = i.FinalPrice.Amount,
                    VatRate = i.VatRate,
                    VatAmount = i.VatAmount.Amount,
                    PromotionId = i.PromotionId,
                    PromotionName = i.PromotionName,
                    DiscountAmount = i.DiscountAmount
                }).ToList(),
                Subtotal = order.Subtotal.Amount,
                VatTotal = order.VatTotal.Amount,
                Total = order.Total.Amount,
                Currency = order.Currency,
                CreatedAt = clock.UtcNow
            },
            Contracts.Common.EventTypes.OrderCreated,
            cancellationToken);

        return Result.Success(order.Id);
    }
}

