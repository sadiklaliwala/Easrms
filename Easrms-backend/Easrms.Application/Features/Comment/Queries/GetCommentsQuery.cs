using Easrms.Application.DTOs.Comment;
using Easrms.Application.Interfaces.Repositories;
using MediatR;

namespace Easrms.Application.Features.Comment.Queries;

/// <summary>
/// Returns all non-deleted comments for a given service request.
/// </summary>
public sealed class GetCommentsQuery : IRequest<IReadOnlyList<CommentListDto>>
{
    public Guid RequestId { get; init; }
}


/// <summary>
/// Step-by-step per HANDLER_REPO_REFERENCE_MAP:
///   1. IRequestRepository.ExistsAsync(requestId)              → 404 if false
///   2. ICommentRepository.GetCommentsByRequestIdAsync(requestId)
///      → returns IReadOnlyList&lt;CommentListDto&gt; directly (Dapper projection)
/// </summary>
public sealed class GetCommentsQueryHandler : IRequestHandler<GetCommentsQuery, IReadOnlyList<CommentListDto>>
{
    private readonly ICommentRepository _commentRepository;
    private readonly IRequestRepository _requestRepository;

    public GetCommentsQueryHandler(
        ICommentRepository commentRepository,
        IRequestRepository requestRepository)
    {
        _commentRepository = commentRepository;
        _requestRepository = requestRepository;
    }

    public async Task<IReadOnlyList<CommentListDto>> Handle(
        GetCommentsQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Guard — ensures we return 404 rather than an empty list for a missing request
        var requestExists = await _requestRepository.ExistsAsync(
            request.RequestId,
            cancellationToken: cancellationToken);

        if (!requestExists)
            throw new KeyNotFoundException(
                $"Service request with id '{request.RequestId}' was not found.");

        // 2. Fetch comments — Dapper projection, returns DTO list directly
        return await _commentRepository.GetCommentsByRequestIdAsync(
            request.RequestId,
            cancellationToken: cancellationToken);
    }
}