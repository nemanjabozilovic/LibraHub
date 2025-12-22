using LibraHub.Catalog.Application.Abstractions;
using LibraHub.Catalog.Domain.Promotions;
using LibraHub.Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LibraHub.Catalog.Infrastructure.Repositories;

public class PromotionRepository : IPromotionRepository
{
    private readonly CatalogDbContext _context;

    public PromotionRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<PromotionCampaign?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.PromotionCampaigns
            .Include(c => c.Rules)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<List<PromotionCampaign>> GetActiveAsync(DateTime utcNow, CancellationToken cancellationToken = default)
    {
        return await _context.PromotionCampaigns
            .Include(c => c.Rules)
            .Where(c => c.Status == PromotionStatus.Active
                && c.StartsAtUtc <= utcNow
                && c.EndsAtUtc >= utcNow)
            .OrderByDescending(c => c.Priority)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<PromotionCampaign>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _context.PromotionCampaigns
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PromotionCampaigns.CountAsync(cancellationToken);
    }

    public async Task AddAsync(PromotionCampaign campaign, CancellationToken cancellationToken = default)
    {
        await _context.PromotionCampaigns.AddAsync(campaign, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(PromotionCampaign campaign, CancellationToken cancellationToken = default)
    {
        _context.PromotionCampaigns.Update(campaign);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddAuditAsync(PromotionAudit audit, CancellationToken cancellationToken = default)
    {
        await _context.PromotionAudits.AddAsync(audit, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
