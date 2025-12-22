using LibraHub.BuildingBlocks.Results;
using MediatR;

namespace LibraHub.Catalog.Application.Promotions.Queries.GetCampaigns;

public record GetCampaignsQuery(
    int Page = 1,
    int PageSize = 20) : IRequest<Result<GetCampaignsResponseDto>>;
