using LibraHub.Orders.Domain.Orders;

namespace LibraHub.Orders.Application.Abstractions;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default);

    Task<Order?> GetByIdAndUserIdAsync(Guid orderId, Guid userId, CancellationToken cancellationToken = default);

    Task<List<Order>> GetByUserIdAsync(Guid userId, int skip, int take, CancellationToken cancellationToken = default);

    Task<int> CountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<int> CountAllAsync(CancellationToken cancellationToken = default);

    Task<int> CountByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default);

    Task<OrderPeriodStatistics> GetStatisticsForPeriodAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);

    Task<OrderRevenue> GetTotalRevenueAsync(CancellationToken cancellationToken = default);

    Task AddAsync(Order order, CancellationToken cancellationToken = default);

    Task UpdateAsync(Order order, CancellationToken cancellationToken = default);
}

public class OrderPeriodStatistics
{
    public int Count { get; init; }
    public decimal Revenue { get; init; }
    public string Currency { get; init; } = string.Empty;
}

public class OrderRevenue
{
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
}

