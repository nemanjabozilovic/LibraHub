using LibraHub.BuildingBlocks.Abstractions;
using LibraHub.BuildingBlocks.Results;
using LibraHub.Catalog.Application.Abstractions;
using LibraHub.Catalog.Domain.Errors;
using LibraHub.Catalog.Domain.Promotions;
using MediatR;
using Error = LibraHub.BuildingBlocks.Results.Error;

namespace LibraHub.Catalog.Application.Promotions.Commands.ActivateCampaign;

public class ActivateCampaignHandler(
    IPromotionRepository promotionRepository,
    IClock clock) : IRequestHandler<ActivateCampaignCommand, Result>
{
    public async Task<Result> Handle(ActivateCampaignCommand request, CancellationToken cancellationToken)
    {
        var campaign = await promotionRepository.GetByIdAsync(request.CampaignId, cancellationToken);
        if (campaign == null)
        {
            return Result.Failure(Error.NotFound(CatalogErrors.Promotion.NotFound));
        }

        try
        {
            campaign.Activate(clock.UtcNow);
            await promotionRepository.UpdateAsync(campaign, cancellationToken);

            var audit = new PromotionAudit(
                Guid.NewGuid(),
                campaign.Id,
                "Activated",
                request.ActorUserId);
            await promotionRepository.AddAuditAsync(audit, cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Validation(ex.Message));
        }
    }
}
