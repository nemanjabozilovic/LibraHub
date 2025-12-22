using LibraHub.BuildingBlocks.Results;
using MediatR;

namespace LibraHub.Catalog.Application.Promotions.Queries.GetCampaign;

public record GetCampaignQuery(Guid CampaignId) : IRequest<Result<GetCampaignResponseDto>>;
