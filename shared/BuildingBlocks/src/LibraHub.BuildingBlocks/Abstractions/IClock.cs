namespace LibraHub.BuildingBlocks.Abstractions;

public interface IClock
{
    DateTime UtcNow { get; }
    DateTimeOffset UtcNowOffset { get; }
}
