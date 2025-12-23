using MediatR;

namespace LibraHub.Library.Application.Reading.Queries.GetProgress;

public class GetProgressQuery : IRequest<LibraHub.BuildingBlocks.Results.Result<ReadingProgressDto>>
{
    public Guid BookId { get; init; }
}

