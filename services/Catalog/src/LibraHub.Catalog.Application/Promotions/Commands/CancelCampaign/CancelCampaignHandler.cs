using LibraHub.BuildingBlocks.Results;
using LibraHub.Catalog.Application.Abstractions;
using LibraHub.Catalog.Domain.Errors;
using LibraHub.Catalog.Domain.Promotions;
using MediatR;
using Error = LibraHub.BuildingBlocks.Results.Error;

namespace LibraHub.Catalog.Application.Promotions.Commands.CancelCampaign;

public class CancelCampaignHandler(
    IPromotionRepository promotionRepository) : IRequestHandler<CancelCampaignCommand, Result>
{
    public async Task<Result> Handle(CancelCampaignCommand request, CancellationToken cancellationToken)
    {
        var campaign = await promotionRepository.GetByIdAsync(request.CampaignId, cancellationToken);
        if (campaign == null)
        {
            return Result.Failure(Error.NotFound(CatalogErrors.Promotion.NotFound));
        }

        try
        {
            campaign.Cancel();
            await promotionRepository.UpdateAsync(campaign, cancellationToken);

            var audit = new PromotionAudit(
                Guid.NewGuid(),
                campaign.Id,
                "Cancelled",
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
