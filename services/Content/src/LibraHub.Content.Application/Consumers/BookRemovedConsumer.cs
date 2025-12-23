using LibraHub.Content.Application.Abstractions;
using LibraHub.Contracts.Catalog.V1;
using Microsoft.Extensions.Logging;

namespace LibraHub.Content.Application.Consumers;

public class BookRemovedConsumer(
    IStoredObjectRepository storedObjectRepository,
    IBookEditionRepository editionRepository,
    ICoverRepository coverRepository,
    ILogger<BookRemovedConsumer> logger)
{
    public async Task HandleAsync(BookRemovedV1 @event, CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing BookRemoved event for BookId: {BookId}, Reason: {Reason}", @event.BookId, @event.Reason);

        var storedObjects = await storedObjectRepository.GetByBookIdAsync(@event.BookId, cancellationToken);
        foreach (var obj in storedObjects)
        {
            obj.Block($"Book removed: {@event.Reason}");
            await storedObjectRepository.UpdateAsync(obj, cancellationToken);
        }

        var editions = await editionRepository.GetByBookIdAsync(@event.BookId, cancellationToken);
        foreach (var edition in editions)
        {
            edition.Block();
            await editionRepository.UpdateAsync(edition, cancellationToken);
        }

        var cover = await coverRepository.GetByBookIdAsync(@event.BookId, cancellationToken);
        if (cover != null)
        {
            cover.Block();
            await coverRepository.UpdateAsync(cover, cancellationToken);
        }

        logger.LogInformation("All content blocked for BookId: {BookId}", @event.BookId);
    }
}

