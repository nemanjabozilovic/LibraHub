using LibraHub.BuildingBlocks.Abstractions;
using LibraHub.BuildingBlocks.Results;
using LibraHub.Orders.Application.Abstractions;
using LibraHub.Orders.Domain.Errors;
using LibraHub.Orders.Domain.Refunds;
using MediatR;
using Error = LibraHub.BuildingBlocks.Results.Error;

namespace LibraHub.Orders.Application.Orders.Commands.RefundOrder;

public class RefundOrderHandler(
    IOrderRepository orderRepository,
    IPaymentRepository paymentRepository,
    IRefundRepository refundRepository,
    IOutboxWriter outboxWriter,
    ICurrentUser currentUser,
    IClock clock,
    IUnitOfWork unitOfWork) : IRequestHandler<RefundOrderCommand, Result>
{
    public async Task<Result> Handle(RefundOrderCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.UserId.HasValue)
        {
            return Result.Failure(Error.Unauthorized(OrdersErrors.User.NotAuthenticated));
        }

        var refundedBy = currentUser.UserId.Value;

        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
        {
            return Result.Failure(Error.NotFound(OrdersErrors.Order.NotFound));
        }

        if (!order.CanBeRefunded)
        {
            return Result.Failure(Error.Validation(OrdersErrors.Order.CannotRefund));
        }

        var payment = await paymentRepository.GetByOrderIdAsync(order.Id, cancellationToken);
        if (payment == null)
        {
            return Result.Failure(Error.NotFound(OrdersErrors.Payment.NotFound));
        }

        var refund = new Refund(
            Guid.NewGuid(),
            order.Id,
            payment.Id,
            request.Reason,
            refundedBy);

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            await refundRepository.AddAsync(refund, cancellationToken);

            order.MarkAsRefunded();
            await orderRepository.UpdateAsync(order, cancellationToken);

            await outboxWriter.WriteAsync(
                new Contracts.Orders.V1.OrderRefundedV1
                {
                    OrderId = order.Id,
                    UserId = order.UserId,
                    RefundId = refund.Id,
                    Reason = request.Reason,
                    RefundedBy = refundedBy,
                    RefundedAt = clock.UtcNow
                },
                Contracts.Common.EventTypes.OrderRefunded,
                cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success();
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

