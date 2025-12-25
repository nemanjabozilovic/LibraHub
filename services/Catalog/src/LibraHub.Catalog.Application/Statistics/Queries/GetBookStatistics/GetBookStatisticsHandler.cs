using LibraHub.BuildingBlocks.Abstractions;
using LibraHub.BuildingBlocks.Results;
using LibraHub.Catalog.Application.Abstractions;
using LibraHub.Catalog.Domain.Books;
using MediatR;

namespace LibraHub.Catalog.Application.Statistics.Queries.GetBookStatistics;

public class GetBookStatisticsHandler(
    IBookRepository bookRepository,
    IClock clock) : IRequestHandler<GetBookStatisticsQuery, Result<BookStatisticsDto>>
{
    public async Task<Result<BookStatisticsDto>> Handle(GetBookStatisticsQuery request, CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var last30Days = now.AddDays(-30);

        var totalTask = bookRepository.CountAllAsync(cancellationToken);
        var publishedTask = bookRepository.CountByStatusAsync(BookStatus.Published, cancellationToken);
        var draftTask = bookRepository.CountByStatusAsync(BookStatus.Draft, cancellationToken);
        var unlistedTask = bookRepository.CountByStatusAsync(BookStatus.Unlisted, cancellationToken);
        var newLast30DaysTask = bookRepository.CountCreatedAfterAsync(last30Days, cancellationToken);

        await Task.WhenAll(totalTask, publishedTask, draftTask, unlistedTask, newLast30DaysTask);

        var response = new BookStatisticsDto
        {
            Total = await totalTask,
            Published = await publishedTask,
            Draft = await draftTask,
            Unlisted = await unlistedTask,
            NewLast30Days = await newLast30DaysTask
        };

        return Result.Success(response);
    }
}

