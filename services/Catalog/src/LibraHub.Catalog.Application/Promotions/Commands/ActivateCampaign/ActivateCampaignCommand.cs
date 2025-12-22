using LibraHub.BuildingBlocks.Results;
using MediatR;

namespace LibraHub.Catalog.Application.Promotions.Commands.ActivateCampaign;

public record ActivateCampaignCommand(
    Guid CampaignId,
    Guid ActorUserId) : IRequest<Result>;
