using Easrms.Application.Interfaces.Repositories;
using Easrms.Common.Constants;
using Easrms.Domain.Entities;
using MediatR;

namespace Easrms.Application.Features.Request.Commands;

/// <summary>
/// Manager approves or rejects a request that is in Pending Approval status.
/// Action must be "Approve" or "Reject".
/// Comment is mandatory when Action is "Reject".
/// </summary>
public sealed class ApprovalRequestCommand : IRequest
{
    public Guid RequestId { get; init; }
    public string Action { get; init; } = string.Empty;   // "Approve" | "Reject"
    public string Comment { get; init; } = string.Empty;

    /// <summary>Manager's UserId extracted from JWT claims by the controller.</summary>
    public Guid CurrentUserId { get; init; }
}


/// <summary>
/// Step-by-step per HANDLER_REPO_REFERENCE_MAP:
///   1. GetRequestByIdAsync(requestId)
///      → null                        = 404
///      → Status != PendingApproval   = 409 invalid transition
///   2. entity.Employee.ManagerId == currentUserId
///      → mismatch                    = 403
///   3. Apply status:
///      "Approve" → entity.Status = Approved
///      "Reject"  → entity.Status = Rejected, entity.RejectionReason = comment (mandatory)
///   4. entity.UpdatedOn = UtcNow
///   5. IRequestRepository.Update(entity)
///   6. ICommentRepository.AddStatusHistoryAsync(history)
///      → OldStatus = PendingApproval, NewStatus = new status, Remarks = comment
///   7. If comment not empty → ICommentRepository.AddCommentAsync(comment entity)
///   8. IRequestRepository.SaveChangesAsync()   ← single commit for all staged changes
/// </summary>
public sealed class ApprovalRequestCommandHandler(
    IRequestRepository requestRepository,
    ICommentRepository commentRepository) : IRequestHandler<ApprovalRequestCommand>
{
    private readonly IRequestRepository _requestRepository = requestRepository;
    private readonly ICommentRepository _commentRepository = commentRepository;

    public async Task Handle(
        ApprovalRequestCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Fetch request — needs Employee navigation to check ManagerId
        var entity = await _requestRepository.GetRequestByIdAsync(
            request.RequestId,
            cancellationToken: cancellationToken)
            ?? throw new KeyNotFoundException(
                $"Service request with id '{request.RequestId}' was not found.");

        if (entity.Status != StatusConstants.PendingApproval)
            throw new InvalidOperationException(
                $"Request '{entity.RequestNumber}' is not in Pending Approval status " +
                $"and cannot be approved or rejected. Current status: {entity.Status}.");

        // 2. Manager scope check — caller must be the employee's direct manager
        if (entity.Employee.ManagerId != request.CurrentUserId)
            throw new UnauthorizedAccessException(
                "You are not the assigned manager for this request's owner.");

        // 3. Apply action
        var oldStatus = entity.Status;

        if (request.Action == "Approve")
        {
            entity.Status = StatusConstants.Approved;
        }
        else if (request.Action == "Reject")
        {
            if (string.IsNullOrWhiteSpace(request.Comment))
                throw new InvalidOperationException(
                    "A comment is mandatory when rejecting a request.");

            entity.Status = StatusConstants.Rejected;
            entity.RejectionReason = request.Comment;
        }
        else
        {
            throw new InvalidOperationException(
                $"Unknown approval action '{request.Action}'. Expected 'Approve' or 'Reject'.");
        }

        // 4. Timestamp
        entity.UpdatedOn = DateTime.UtcNow;

        // 5. Mark dirty
        _requestRepository.Update(entity);

        // 6. Stage history entry
        var history = new RequestStatusHistory
        {
            HistoryId = Guid.NewGuid(),
            RequestId = entity.RequestId,
            OldStatus = oldStatus,
            NewStatus = entity.Status,
            ChangedBy = request.CurrentUserId,
            ChangedOn = DateTime.UtcNow,
            Remarks = request.Comment
        };

        await _commentRepository.AddStatusHistoryAsync(history, cancellationToken);

        // 7. Stage comment if the manager provided one
        if (!string.IsNullOrWhiteSpace(request.Comment))
        {
            var comment = new RequestComment
            {
                CommentId = Guid.NewGuid(),
                RequestId = entity.RequestId,
                CommentBy = request.CurrentUserId,
                CommentText = request.Comment,
                CommentType = CommentTypeConstants.Approval,
                CreatedOn = DateTime.UtcNow,
                IsDeleted = false
            };

            await _commentRepository.AddCommentAsync(comment, cancellationToken);
        }

        // 8. Single commit — history + optional comment + request update all in one transaction
        await _requestRepository.SaveChangesAsync(cancellationToken);
    }
}