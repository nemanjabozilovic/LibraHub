using LibraHub.BuildingBlocks.Abstractions;
using LibraHub.BuildingBlocks.Results;
using LibraHub.Content.Application.Abstractions;
using LibraHub.Content.Application.Options;
using LibraHub.Content.Domain.Books;
using LibraHub.Content.Domain.Errors;
using LibraHub.Content.Domain.Storage;
using MediatR;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using Error = LibraHub.BuildingBlocks.Results.Error;

namespace LibraHub.Content.Application.Upload.Commands.UploadEdition;

public class UploadEditionHandler(
    IObjectStorage objectStorage,
    IStoredObjectRepository storedObjectRepository,
    IBookEditionRepository editionRepository,
    ICatalogReadClient catalogClient,
    IOutboxWriter outboxWriter,
    IClock clock,
    IOptions<UploadOptions> uploadOptions) : IRequestHandler<UploadEditionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UploadEditionCommand request, CancellationToken cancellationToken)
    {
        // Check if book exists and is not blocked
        var bookInfo = await catalogClient.GetBookInfoAsync(request.BookId, cancellationToken);
        if (bookInfo == null)
        {
            return Result.Failure<Guid>(Error.NotFound(ContentErrors.Book.NotFound));
        }

        if (bookInfo.IsBlocked)
        {
            return Result.Failure<Guid>(Error.Validation(ContentErrors.Book.Blocked));
        }

        // Parse format
        if (!Enum.TryParse<BookFormat>(request.Format, ignoreCase: true, out var format))
        {
            return Result.Failure<Guid>(Error.Validation(ContentErrors.Edition.InvalidFormat));
        }

        // Determine version
        int version;
        if (request.Version.HasValue)
        {
            version = request.Version.Value;
        }
        else
        {
            // Get latest version and increment
            var latest = await editionRepository.GetLatestByBookIdAndFormatAsync(request.BookId, format, cancellationToken);
            version = latest != null ? latest.Version + 1 : 1;
        }

        // Check if this version already exists
        var existing = await editionRepository.GetByBookIdFormatAndVersionAsync(request.BookId, format, version, cancellationToken);
        if (existing != null)
        {
            return Result.Failure<Guid>(Error.Validation(ContentErrors.Edition.InvalidVersion));
        }

        // Compute SHA-256 checksum
        string sha256;
        using (var stream = request.File.OpenReadStream())
        {
            sha256 = await ComputeSha256Async(stream, cancellationToken);
        }

        // Generate object key
        var fileExtension = format == BookFormat.Pdf ? "pdf" : "epub";
        var objectKey = $"books/{request.BookId}/editions/{format.ToString().ToLowerInvariant()}/v{version}/{Guid.NewGuid()}.{fileExtension}";

        // Upload to object storage
        try
        {
            await objectStorage.UploadAsync(
                uploadOptions.Value.EditionsBucketName,
                objectKey,
                request.File.OpenReadStream(),
                request.File.ContentType,
                cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>(new Error("INTERNAL_ERROR", $"{ContentErrors.Storage.UploadFailed}: {ex.Message}"));
        }

        // Create stored object
        var storedObject = new StoredObject(
            Guid.NewGuid(),
            request.BookId,
            objectKey,
            request.File.ContentType,
            request.File.Length,
            new Sha256(sha256));

        await storedObjectRepository.AddAsync(storedObject, cancellationToken);

        // Create edition
        var edition = new BookEdition(
            Guid.NewGuid(),
            request.BookId,
            format,
            version,
            storedObject.Id);

        await editionRepository.AddAsync(edition, cancellationToken);

        // Publish event
        await outboxWriter.WriteAsync(
            new Contracts.Content.V1.EditionUploadedV1
            {
                BookId = request.BookId,
                Format = format.ToString(),
                Version = version,
                EditionRef = objectKey,
                Sha256 = sha256,
                Size = request.File.Length,
                ContentType = request.File.ContentType,
                UploadedAt = clock.UtcNow
            },
            Contracts.Common.EventTypes.EditionUploaded,
            cancellationToken);

        return Result.Success(edition.Id);
    }

    private static async Task<string> ComputeSha256Async(Stream stream, CancellationToken cancellationToken)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = await sha256.ComputeHashAsync(stream, cancellationToken);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}

