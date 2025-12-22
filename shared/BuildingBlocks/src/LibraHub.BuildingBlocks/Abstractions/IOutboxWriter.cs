namespace LibraHub.BuildingBlocks.Abstractions;

public interface IOutboxWriter
{
    Task WriteAsync<T>(T integrationEvent, string eventType, CancellationToken cancellationToken = default) where T : class;
}
