using LibraHub.Orders.Application.Abstractions;
using LibraHub.Orders.Domain.Orders;
using LibraHub.Orders.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LibraHub.Orders.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly OrdersDbContext _context;

    public OrderRepository(OrdersDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
    }

    public async Task<Order?> GetByIdAndUserIdAsync(Guid orderId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId, cancellationToken);
    }

    public async Task<List<Order>> GetByUserIdAsync(Guid userId, int skip, int take, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .CountAsync(o => o.UserId == userId, cancellationToken);
    }

    public async Task<int> CountAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Orders.CountAsync(cancellationToken);
    }

    public async Task<int> CountByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .CountAsync(o => o.Status == status, cancellationToken);
    }

    public async Task<OrderPeriodStatistics> GetStatisticsForPeriodAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var orders = await _context.Orders
            .Where(o => o.Status == OrderStatus.Paid
                && o.CreatedAt >= from
                && o.CreatedAt <= to)
            .ToListAsync(cancellationToken);

        var count = orders.Count;
        var revenue = orders.Sum(o => o.Total.Amount);
        var currency = orders.FirstOrDefault()?.Currency ?? "USD";

        return new OrderPeriodStatistics
        {
            Count = count,
            Revenue = revenue,
            Currency = currency
        };
    }

    public async Task<OrderRevenue> GetTotalRevenueAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _context.Orders
            .Where(o => o.Status == OrderStatus.Paid)
            .ToListAsync(cancellationToken);

        var amount = orders.Sum(o => o.Total.Amount);
        var currency = orders.FirstOrDefault()?.Currency ?? "USD";

        return new OrderRevenue
        {
            Amount = amount,
            Currency = currency
        };
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        await _context.Orders.AddAsync(order, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

