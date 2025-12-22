using LibraHub.BuildingBlocks.Results;
using LibraHub.Catalog.Application.Abstractions;
using LibraHub.Catalog.Domain.Errors;
using LibraHub.Catalog.Domain.Promotions;
using MediatR;
using Error = LibraHub.BuildingBlocks.Results.Error;

namespace LibraHub.Catalog.Application.Promotions.Commands.EndCampaign;

public class EndCampaignHandler(
    IPromotionRepository promotionRepository) : IRequestHandler<EndCampaignCommand, Result>
{
    public async Task<Result> Handle(EndCampaignCommand request, CancellationToken cancellationToken)
    {
        var campaign = await promotionRepository.GetByIdAsync(request.CampaignId, cancellationToken);
        if (campaign == null)
        {
            return Result.Failure(Error.NotFound(CatalogErrors.Promotion.NotFound));
        }

        try
        {
            campaign.End();
            await promotionRepository.UpdateAsync(campaign, cancellationToken);

            var audit = new PromotionAudit(
                Guid.NewGuid(),
                campaign.Id,
                "Ended",
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
