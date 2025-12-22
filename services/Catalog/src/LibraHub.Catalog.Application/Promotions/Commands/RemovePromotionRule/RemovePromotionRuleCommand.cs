using LibraHub.BuildingBlocks.Results;
using MediatR;

namespace LibraHub.Catalog.Application.Promotions.Commands.RemovePromotionRule;

public record RemovePromotionRuleCommand(
    Guid CampaignId,
    Guid RuleId) : IRequest<Result>;
