using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;

namespace LibraHub.BuildingBlocks.Outbox;

public abstract class OutboxPublisherWorker : BackgroundService
{
    private readonly ILogger<OutboxPublisherWorker> _logger;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _exchangeName;

    protected OutboxPublisherWorker(
        ILogger<OutboxPublisherWorker> logger,
        IConnection connection,
        string exchangeName)
    {
        _logger = logger;
        _connection = connection;
        _exchangeName = exchangeName;
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(exchangeName, ExchangeType.Topic, durable: true);
    }

    protected abstract Task<List<OutboxMessage>> GetPendingMessagesAsync(int batchSize, CancellationToken cancellationToken);

    protected abstract Task MarkAsProcessedAsync(Guid messageId, CancellationToken cancellationToken);

    protected abstract Task MarkAsFailedAsync(Guid messageId, string error, CancellationToken cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(cancellationToken);
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox messages");
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            }
        }
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        const int batchSize = 50;
        var messages = await GetPendingMessagesAsync(batchSize, cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                var routingKey = message.EventType;
                var body = Encoding.UTF8.GetBytes(message.Payload);

                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;
                properties.MessageId = message.Id.ToString();
                properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                properties.Type = message.EventType;

                _channel.BasicPublish(
                    exchange: _exchangeName,
                    routingKey: routingKey,
                    basicProperties: properties,
                    body: body);

                await MarkAsProcessedAsync(message.Id, cancellationToken);
                _logger.LogInformation("Published outbox message {MessageId} of type {EventType}", message.Id, message.EventType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing outbox message {MessageId}", message.Id);
                await MarkAsFailedAsync(message.Id, ex.Message, cancellationToken);
            }
        }
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
