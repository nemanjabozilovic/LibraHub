using LibraHub.BuildingBlocks.Results;
using LibraHub.Catalog.Application.Abstractions;
using LibraHub.Catalog.Domain.Errors;
using MediatR;
using Error = LibraHub.BuildingBlocks.Results.Error;

namespace LibraHub.Catalog.Application.Promotions.Commands.RemovePromotionRule;

public class RemovePromotionRuleHandler(
    IPromotionRepository promotionRepository) : IRequestHandler<RemovePromotionRuleCommand, Result>
{
    public async Task<Result> Handle(RemovePromotionRuleCommand request, CancellationToken cancellationToken)
    {
        var campaign = await promotionRepository.GetByIdAsync(request.CampaignId, cancellationToken);
        if (campaign == null)
        {
            return Result.Failure(Error.NotFound(CatalogErrors.Promotion.NotFound));
        }

        try
        {
            campaign.RemoveRule(request.RuleId);
            await promotionRepository.UpdateAsync(campaign, cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Validation(ex.Message));
        }
    }
}
