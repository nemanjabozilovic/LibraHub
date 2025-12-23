namespace LibraHub.Library.Application.Reading.Queries.GetProgress;

public class ReadingProgressDto
{
    public Guid BookId { get; init; }
    public decimal Percentage { get; init; }
    public int? LastPage { get; init; }
    public DateTime LastUpdatedAt { get; init; }
}

