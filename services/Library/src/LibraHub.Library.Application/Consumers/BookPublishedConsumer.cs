using LibraHub.Contracts.Catalog.V1;
using LibraHub.Library.Application.Abstractions;
using LibraHub.Library.Domain.Books;
using Microsoft.Extensions.Logging;

namespace LibraHub.Library.Application.Consumers;

public class BookPublishedConsumer(
    IBookSnapshotStore bookSnapshotStore,
    ILogger<BookPublishedConsumer> logger)
{
    public async Task HandleAsync(BookPublishedV1 @event, CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing BookPublished event for BookId: {BookId}", @event.BookId);

        var existing = await bookSnapshotStore.GetByIdAsync(@event.BookId, cancellationToken);

        if (existing != null)
        {
            // Update existing snapshot
            existing.Update(@event.Title, "Unknown Author"); // Authors not in BookPublished event
            await bookSnapshotStore.AddOrUpdateAsync(existing, cancellationToken);
        }
        else
        {
            // Create new snapshot
            var snapshot = new BookSnapshot(
                @event.BookId,
                @event.Title,
                "Unknown Author"); // Authors not in BookPublished event

            await bookSnapshotStore.AddOrUpdateAsync(snapshot, cancellationToken);
        }

        logger.LogInformation("Updated book snapshot for BookId: {BookId}", @event.BookId);
    }
}

