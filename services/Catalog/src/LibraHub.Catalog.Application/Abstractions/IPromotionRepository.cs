using LibraHub.Catalog.Domain.Promotions;

namespace LibraHub.Catalog.Application.Abstractions;

public interface IPromotionRepository
{
    Task<PromotionCampaign?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<PromotionCampaign>> GetActiveAsync(DateTime utcNow, CancellationToken cancellationToken = default);

    Task<List<PromotionCampaign>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    Task<int> CountAllAsync(CancellationToken cancellationToken = default);

    Task AddAsync(PromotionCampaign campaign, CancellationToken cancellationToken = default);

    Task UpdateAsync(PromotionCampaign campaign, CancellationToken cancellationToken = default);

    Task AddAuditAsync(PromotionAudit audit, CancellationToken cancellationToken = default);
}
