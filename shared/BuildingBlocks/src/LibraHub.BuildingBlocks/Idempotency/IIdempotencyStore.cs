namespace LibraHub.BuildingBlocks.Idempotency;

public interface IIdempotencyStore
{
    Task<IdempotencyResponse?> GetResponseAsync(string idempotencyKey, CancellationToken cancellationToken = default);

    Task StoreResponseAsync(string idempotencyKey, int statusCode, string contentType, byte[] body, CancellationToken cancellationToken = default);
}

public class IdempotencyResponse
{
    public int StatusCode { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public byte[] Body { get; set; } = [];
}
