using LibraHub.BuildingBlocks.Abstractions;
using LibraHub.BuildingBlocks.Results;
using LibraHub.Library.Application.Abstractions;
using LibraHub.Library.Domain.Entitlements;
using MediatR;

namespace LibraHub.Library.Application.Statistics.Queries.GetEntitlementStatistics;

public class GetEntitlementStatisticsHandler(
    IEntitlementRepository entitlementRepository,
    IClock clock) : IRequestHandler<GetEntitlementStatisticsQuery, Result<EntitlementStatisticsDto>>
{
    public async Task<Result<EntitlementStatisticsDto>> Handle(GetEntitlementStatisticsQuery request, CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var last30Days = now.AddDays(-30);

        var totalTask = entitlementRepository.CountAllAsync(cancellationToken);
        var activeTask = entitlementRepository.CountByStatusAsync(EntitlementStatus.Active, cancellationToken);
        var revokedTask = entitlementRepository.CountByStatusAsync(EntitlementStatus.Revoked, cancellationToken);
        var grantedLast30DaysTask = entitlementRepository.CountAcquiredAfterAsync(last30Days, cancellationToken);

        await Task.WhenAll(totalTask, activeTask, revokedTask, grantedLast30DaysTask);

        var response = new EntitlementStatisticsDto
        {
            Total = await totalTask,
            Active = await activeTask,
            Revoked = await revokedTask,
            GrantedLast30Days = await grantedLast30DaysTask
        };

        return Result.Success(response);
    }
}

