using LibraHub.BuildingBlocks.Abstractions;
using LibraHub.BuildingBlocks.Results;
using LibraHub.Library.Application.Abstractions;
using LibraHub.Library.Domain.Errors;
using LibraHub.Library.Domain.Reading;
using MediatR;
using Error = LibraHub.BuildingBlocks.Results.Error;

namespace LibraHub.Library.Application.Reading.Commands.UpdateProgress;

public class UpdateProgressHandler(
    IReadingProgressRepository progressRepository,
    IEntitlementRepository entitlementRepository,
    ICurrentUser currentUser) : IRequestHandler<UpdateProgressCommand, Result>
{
    public async Task<Result> Handle(UpdateProgressCommand request, CancellationToken cancellationToken)
    {
        var userIdResult = currentUser.RequireUserId(LibraryErrors.User.NotAuthenticated);
        if (userIdResult.IsFailure)
        {
            return Result.Failure(userIdResult.Error!);
        }

        var userId = userIdResult.Value;

        var hasAccess = await entitlementRepository.HasAccessAsync(userId, request.BookId, cancellationToken);
        if (!hasAccess)
        {
            return Result.Failure(Error.Validation("User does not have access to this book"));
        }

        var progress = await progressRepository.GetByUserAndBookAsync(userId, request.BookId, cancellationToken);

        if (progress == null)
        {
            progress = new ReadingProgress(Guid.NewGuid(), userId, request.BookId);
            progress.UpdateProgress(request.Percentage, request.LastPage);
            await progressRepository.AddAsync(progress, cancellationToken);
        }
        else
        {
            progress.UpdateProgress(request.Percentage, request.LastPage);
            await progressRepository.UpdateAsync(progress, cancellationToken);
        }

        return Result.Success();
    }
}

