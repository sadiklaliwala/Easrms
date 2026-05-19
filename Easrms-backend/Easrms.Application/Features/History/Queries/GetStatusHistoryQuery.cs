using Easrms.Application.DTOs.Comment;
using Easrms.Application.Interfaces.Repositories;
using MediatR;

namespace Easrms.Application.Features.History.Queries;

/// <summary>
/// Returns the full status change audit trail for a service request,
/// ordered chronologically ascending.
/// </summary>
public sealed class GetStatusHistoryQuery : IRequest<IReadOnlyList<StatusHistoryDto>>
{
    public Guid RequestId { get; init; }
}


/// <summary>
/// Step-by-step per HANDLER_REPO_REFERENCE_MAP:
///   1. IRequestRepository.ExistsAsync(requestId)                    → 404 if false
///   2. ICommentRepository.GetStatusHistoryByRequestIdAsync(requestId)
///      → returns IReadOnlyList&lt;StatusHistoryDto&gt; directly (Dapper projection)
/// </summary>
public sealed class GetStatusHistoryQueryHandler : IRequestHandler<GetStatusHistoryQuery, IReadOnlyList<StatusHistoryDto>>
{
    private readonly ICommentRepository _commentRepository;
    private readonly IRequestRepository _requestRepository;

    public GetStatusHistoryQueryHandler(
        ICommentRepository commentRepository,
        IRequestRepository requestRepository)
    {
        _commentRepository = commentRepository;
        _requestRepository = requestRepository;
    }

    public async Task<IReadOnlyList<StatusHistoryDto>> Handle(
        GetStatusHistoryQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Guard — 404 before attempting to query history for a ghost request
        var requestExists = await _requestRepository.ExistsAsync(
            request.RequestId,
            cancellationToken: cancellationToken);

        if (!requestExists)
            throw new KeyNotFoundException(
                $"Service request with id '{request.RequestId}' was not found.");

        // 2. Fetch history — Dapper projection, returns DTO list directly
        return await _commentRepository.GetStatusHistoryByRequestIdAsync(
            request.RequestId,
            cancellationToken: cancellationToken);
    }
}