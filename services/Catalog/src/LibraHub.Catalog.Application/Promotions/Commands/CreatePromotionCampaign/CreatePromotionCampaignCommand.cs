using LibraHub.BuildingBlocks.Results;
using LibraHub.Catalog.Domain.Promotions;
using MediatR;

namespace LibraHub.Catalog.Application.Promotions.Commands.CreatePromotionCampaign;

public record CreatePromotionCampaignCommand(
    string Name,
    string? Description,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    StackingPolicy StackingPolicy,
    int Priority,
    Guid CreatedBy) : IRequest<Result<Guid>>;
