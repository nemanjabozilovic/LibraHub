using LibraHub.BuildingBlocks.Results;
using MediatR;

namespace LibraHub.Catalog.Application.Promotions.Commands.CancelCampaign;

public record CancelCampaignCommand(
    Guid CampaignId,
    Guid ActorUserId) : IRequest<Result>;
