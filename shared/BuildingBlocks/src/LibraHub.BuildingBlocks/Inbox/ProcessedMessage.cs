namespace LibraHub.BuildingBlocks.Inbox;

public class ProcessedMessage
{
    public Guid Id { get; set; }
    public string MessageId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
}
