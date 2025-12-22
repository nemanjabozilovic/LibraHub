using LibraHub.BuildingBlocks.Results;
using LibraHub.Catalog.Domain.Promotions;
using MediatR;

namespace LibraHub.Catalog.Application.Promotions.Commands.AddPromotionRule;

public record AddPromotionRuleCommand(
    Guid CampaignId,
    DiscountType DiscountType,
    decimal DiscountValue,
    string? Currency,
    decimal? MinPriceAfterDiscount,
    decimal? MaxDiscountAmount,
    PromotionScope AppliesToScope,
    List<string>? ScopeValues,
    List<Guid>? Exclusions) : IRequest<Result<Guid>>;
