using LibraHub.BuildingBlocks.Abstractions;
using LibraHub.BuildingBlocks.Results;
using LibraHub.Identity.Application.Abstractions;
using LibraHub.Identity.Domain.Users;
using MediatR;

namespace LibraHub.Identity.Application.Statistics.Queries.GetUserStatistics;

public class GetUserStatisticsHandler(
    IUserRepository userRepository,
    IClock clock) : IRequestHandler<GetUserStatisticsQuery, Result<UserStatisticsDto>>
{
    public async Task<Result<UserStatisticsDto>> Handle(GetUserStatisticsQuery request, CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var last30Days = now.AddDays(-30);
        var last7Days = now.AddDays(-7);

        var totalTask = userRepository.CountAllAsync(cancellationToken);
        var activeTask = userRepository.CountByStatusAsync(UserStatus.Active, cancellationToken);
        var disabledTask = userRepository.CountByStatusAsync(UserStatus.Disabled, cancellationToken);
        var pendingTask = userRepository.CountPendingEmailVerificationAsync(cancellationToken);
        var newLast30DaysTask = userRepository.CountCreatedAfterAsync(last30Days, cancellationToken);
        var newLast7DaysTask = userRepository.CountCreatedAfterAsync(last7Days, cancellationToken);

        await Task.WhenAll(totalTask, activeTask, disabledTask, pendingTask, newLast30DaysTask, newLast7DaysTask);

        var response = new UserStatisticsDto
        {
            Total = await totalTask,
            Active = await activeTask,
            Disabled = await disabledTask,
            Pending = await pendingTask,
            NewLast30Days = await newLast30DaysTask,
            NewLast7Days = await newLast7DaysTask
        };

        return Result.Success(response);
    }
}

