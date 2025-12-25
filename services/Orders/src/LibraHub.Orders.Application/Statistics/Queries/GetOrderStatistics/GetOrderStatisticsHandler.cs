using LibraHub.BuildingBlocks.Abstractions;
using LibraHub.BuildingBlocks.Results;
using LibraHub.Orders.Application.Abstractions;
using LibraHub.Orders.Domain.Orders;
using MediatR;

namespace LibraHub.Orders.Application.Statistics.Queries.GetOrderStatistics;

public class GetOrderStatisticsHandler(
    IOrderRepository orderRepository,
    IClock clock) : IRequestHandler<GetOrderStatisticsQuery, Result<OrderStatisticsDto>>
{
    public async Task<Result<OrderStatisticsDto>> Handle(GetOrderStatisticsQuery request, CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var last30Days = now.AddDays(-30);
        var last7Days = now.AddDays(-7);

        var totalTask = orderRepository.CountAllAsync(cancellationToken);
        var paidTask = orderRepository.CountByStatusAsync(OrderStatus.Paid, cancellationToken);
        var pendingTask = orderRepository.CountByStatusAsync(OrderStatus.PaymentPending, cancellationToken);
        var cancelledTask = orderRepository.CountByStatusAsync(OrderStatus.Cancelled, cancellationToken);
        var refundedTask = orderRepository.CountByStatusAsync(OrderStatus.Refunded, cancellationToken);
        var last30DaysStatsTask = orderRepository.GetStatisticsForPeriodAsync(last30Days, now, cancellationToken);
        var last7DaysStatsTask = orderRepository.GetStatisticsForPeriodAsync(last7Days, now, cancellationToken);
        var totalRevenueTask = orderRepository.GetTotalRevenueAsync(cancellationToken);

        await Task.WhenAll(
            totalTask, paidTask, pendingTask, cancelledTask, refundedTask,
            last30DaysStatsTask, last7DaysStatsTask, totalRevenueTask);

        var last30DaysStats = await last30DaysStatsTask;
        var last7DaysStats = await last7DaysStatsTask;
        var totalRevenue = await totalRevenueTask;

        var response = new OrderStatisticsDto
        {
            Total = await totalTask,
            Paid = await paidTask,
            Pending = await pendingTask,
            Cancelled = await cancelledTask,
            Refunded = await refundedTask,
            Last30Days = new PeriodStatistics
            {
                Count = last30DaysStats.Count,
                Revenue = last30DaysStats.Revenue
            },
            Last7Days = new PeriodStatistics
            {
                Count = last7DaysStats.Count,
                Revenue = last7DaysStats.Revenue
            },
            TotalRevenue = totalRevenue.Amount,
            Currency = totalRevenue.Currency
        };

        return Result.Success(response);
    }
}

