using LibraHub.BuildingBlocks.Results;
using LibraHub.Catalog.Application.Abstractions;
using MediatR;

namespace LibraHub.Catalog.Application.Promotions.Queries.GetCampaigns;

public class GetCampaignsHandler(
    IPromotionRepository promotionRepository) : IRequestHandler<GetCampaignsQuery, Result<GetCampaignsResponseDto>>
{
    public async Task<Result<GetCampaignsResponseDto>> Handle(GetCampaignsQuery request, CancellationToken cancellationToken)
    {
        var campaigns = await promotionRepository.GetAllAsync(request.Page, request.PageSize, cancellationToken);
        var totalCount = await promotionRepository.CountAllAsync(cancellationToken);

        var campaignSummaries = campaigns.Select(c => new CampaignSummaryDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            Status = c.Status.ToString(),
            StartsAtUtc = c.StartsAtUtc,
            EndsAtUtc = c.EndsAtUtc,
            StackingPolicy = c.StackingPolicy.ToString(),
            Priority = c.Priority,
            CreatedAt = c.CreatedAt
        }).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var response = new GetCampaignsResponseDto
        {
            Campaigns = campaignSummaries,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = totalPages
        };

        return Result.Success(response);
    }
}
