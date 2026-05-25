using Easrms.Application.Interfaces.Repositories;
using Easrms.Domain.Entities;
using MediatR;
using Easrms.Common.Enums;

namespace Easrms.Application.Features.Comment.Commands;

/// <summary>
/// Adds a comment to an existing service request.
/// CommentBy is always the authenticated user — set by the controller from JWT claims.
/// </summary>
public sealed class AddCommentCommand : IRequest<Guid>
{
    public Guid RequestId { get; init; }
   
    public string CommentText { get; init; } = string.Empty;
    public int CommentType { get; init; }

    public Guid CommentBy { get; set; }
}


/// <summary>
/// Step-by-step per HANDLER_REPO_REFERENCE_MAP:
///   1. IRequestRepository.ExistsAsync(requestId)       → 404 if false
///   2. Construct RequestComment entity
///      → RequestId, CommentBy = currentUserId, CommentText, CommentType,
///        CreatedOn = UtcNow, IsDeleted = false
///   3. ICommentRepository.AddCommentAsync(entity)
///   4. ICommentRepository.SaveChangesAsync()            ← comment-only write owns SaveChanges
///   5. Return new CommentId (201 set by controller)
/// </summary>
public sealed class AddCommentCommandHandler : IRequestHandler<AddCommentCommand, Guid>
{
    private readonly ICommentRepository _commentRepository;
    private readonly IRequestRepository _requestRepository;

    public AddCommentCommandHandler(
        ICommentRepository commentRepository,
        IRequestRepository requestRepository)
    {
        _commentRepository = commentRepository;
        _requestRepository = requestRepository;
    }

    public async Task<Guid> Handle(
        AddCommentCommand request,
        CancellationToken cancellationToken)
    {        
        // 1. Confirm the parent request exists — lightweight scalar check
        var requestExists = await _requestRepository.ExistsAsync(
            request.RequestId,
            cancellationToken: cancellationToken);

        if (!requestExists)
            throw new KeyNotFoundException(
                $"Service request with id '{request.RequestId}' was not found.");

        // 2. Build entity
        var comment = new RequestComment
        {
            RequestId = request.RequestId,
            CommentBy = request.CommentBy,
            CommentText = request.CommentText,
            CommentType = (CommentTypeEnum)request.CommentType,   // store as enum
            CreatedOn = DateTime.UtcNow,
            IsDeleted = false
        };

        // 3. Stage insert
        await _commentRepository.AddCommentAsync(comment, cancellationToken);

        // 4. Commit — comment-only write, so ICommentRepository owns SaveChanges
        await _commentRepository.SaveChangesAsync(cancellationToken);

        // 5. Return id — controller wraps in 201 ApiResponse
        return comment.CommentId;
    }
}