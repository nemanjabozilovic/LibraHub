using LibraHub.BuildingBlocks.Abstractions;
using LibraHub.Content.Application.Abstractions;
using LibraHub.Contracts.Catalog.V1;
using Microsoft.Extensions.Logging;

namespace LibraHub.Content.Application.Consumers;

public class BookRemovedConsumer(
    IStoredObjectRepository storedObjectRepository,
    IBookEditionRepository editionRepository,
    ICoverRepository coverRepository,
    IUnitOfWork unitOfWork,
    ILogger<BookRemovedConsumer> logger)
{
    public async Task HandleAsync(BookRemovedV1 @event, CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing BookRemoved event for BookId: {BookId}, Reason: {Reason}", @event.BookId, @event.Reason);

        var blockReason = $"Book removed: {@event.Reason}";

        var storedObjectsTask = storedObjectRepository.GetByBookIdAsync(@event.BookId, cancellationToken);
        var editionsTask = editionRepository.GetByBookIdAsync(@event.BookId, cancellationToken);
        var coverTask = coverRepository.GetByBookIdAsync(@event.BookId, cancellationToken);

        await Task.WhenAll(storedObjectsTask, editionsTask, coverTask);

        var storedObjects = await storedObjectsTask;
        var editions = await editionsTask;
        var cover = await coverTask;

        foreach (var obj in storedObjects)
        {
            obj.Block(blockReason);
        }

        foreach (var edition in editions)
        {
            edition.Block();
        }

        if (cover != null)
        {
            cover.Block();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("All content blocked for BookId: {BookId}", @event.BookId);
    }
}

