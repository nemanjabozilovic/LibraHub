using LibraHub.Orders.Application.Abstractions;
using LibraHub.Orders.Domain.Refunds;
using LibraHub.Orders.Infrastructure.Persistence;

namespace LibraHub.Orders.Infrastructure.Repositories;

public class RefundRepository : IRefundRepository
{
    private readonly OrdersDbContext _context;

    public RefundRepository(OrdersDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Refund refund, CancellationToken cancellationToken = default)
    {
        await _context.Refunds.AddAsync(refund, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

