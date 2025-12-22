using LibraHub.BuildingBlocks.Results;
using LibraHub.Catalog.Application.Abstractions;
using LibraHub.Catalog.Domain.Errors;
using LibraHub.Catalog.Domain.Promotions;
using MediatR;
using Error = LibraHub.BuildingBlocks.Results.Error;

namespace LibraHub.Catalog.Application.Promotions.Commands.AddPromotionRule;

public class AddPromotionRuleHandler(
    IPromotionRepository promotionRepository) : IRequestHandler<AddPromotionRuleCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(AddPromotionRuleCommand request, CancellationToken cancellationToken)
    {
        var campaign = await promotionRepository.GetByIdAsync(request.CampaignId, cancellationToken);
        if (campaign == null)
        {
            return Result.Failure<Guid>(Error.NotFound(CatalogErrors.Promotion.NotFound));
        }

        try
        {
            var rule = new PromotionRule(
                Guid.NewGuid(),
                request.CampaignId,
                request.DiscountType,
                request.DiscountValue,
                request.Currency,
                request.MinPriceAfterDiscount,
                request.MaxDiscountAmount,
                request.AppliesToScope,
                request.ScopeValues,
                request.Exclusions);

            campaign.AddRule(rule);
            await promotionRepository.UpdateAsync(campaign, cancellationToken);

            return Result.Success(rule.Id);
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>(Error.Validation(ex.Message));
        }
    }
}
