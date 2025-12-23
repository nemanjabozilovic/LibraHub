using LibraHub.Contracts.Catalog.V1;
using LibraHub.Library.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace LibraHub.Library.Application.Consumers;

public class BookUpdatedConsumer(
    IBookSnapshotStore bookSnapshotStore,
    ILogger<BookUpdatedConsumer> logger)
{
    public async Task HandleAsync(BookUpdatedV1 @event, CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing BookUpdated event for BookId: {BookId}", @event.BookId);

        var snapshot = await bookSnapshotStore.GetByIdAsync(@event.BookId, cancellationToken);

        if (snapshot == null)
        {
            logger.LogWarning("Book snapshot not found for BookId: {BookId}, creating new snapshot", @event.BookId);

            // Create new snapshot if it doesn't exist
            var newSnapshot = new Domain.Books.BookSnapshot(
                @event.BookId,
                @event.Title,
                "Unknown Author"); // Authors not in BookUpdated event

            await bookSnapshotStore.AddOrUpdateAsync(newSnapshot, cancellationToken);
        }
        else
        {
            // Update existing snapshot
            snapshot.Update(@event.Title, snapshot.Authors); // Keep existing authors
            await bookSnapshotStore.AddOrUpdateAsync(snapshot, cancellationToken);
        }

        logger.LogInformation("Updated book snapshot for BookId: {BookId}", @event.BookId);
    }
}

