using LibraHub.BuildingBlocks.Abstractions;
using LibraHub.BuildingBlocks.Results;
using LibraHub.Library.Application.Abstractions;
using LibraHub.Library.Domain.Errors;
using MediatR;
using Error = LibraHub.BuildingBlocks.Results.Error;

namespace LibraHub.Library.Application.Entitlements.Queries.MyBooks;

public class MyBooksHandler(
    IEntitlementRepository entitlementRepository,
    IBookSnapshotStore bookSnapshotStore,
    ICurrentUser currentUser) : IRequestHandler<MyBooksQuery, Result<MyBooksDto>>
{
    public async Task<Result<MyBooksDto>> Handle(MyBooksQuery request, CancellationToken cancellationToken)
    {
        if (!currentUser.UserId.HasValue)
        {
            return Result.Failure<MyBooksDto>(Error.Unauthorized(LibraryErrors.User.NotAuthenticated));
        }

        var userId = currentUser.UserId.Value;

        var entitlements = await entitlementRepository.GetActiveByUserIdAsync(userId, cancellationToken);
        var totalCount = entitlements.Count;

        // Apply paging
        var pagedEntitlements = entitlements
            .OrderByDescending(e => e.AcquiredAt)
            .Skip(request.Skip)
            .Take(request.Take)
            .ToList();

        var books = new List<BookDto>();

        foreach (var entitlement in pagedEntitlements)
        {
            var snapshot = await bookSnapshotStore.GetByIdAsync(entitlement.BookId, cancellationToken);

            books.Add(new BookDto
            {
                BookId = entitlement.BookId,
                Title = snapshot?.Title ?? "Unknown Book",
                Authors = snapshot?.Authors ?? "Unknown Author",
                CoverRef = snapshot?.CoverRef,
                AcquiredAt = entitlement.AcquiredAt
            });
        }

        return Result.Success(new MyBooksDto
        {
            Books = books,
            TotalCount = totalCount
        });
    }
}

