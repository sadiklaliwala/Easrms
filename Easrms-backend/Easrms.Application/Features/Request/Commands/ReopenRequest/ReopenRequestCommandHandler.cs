using Easrms.Application.Interfaces.Repositories;
using Easrms.Common.Response;
using Easrms.Common.Enums;
using Easrms.Domain.Entities;
using MediatR;

namespace Easrms.Application.Features.Request.Commands.ReopenRequest;

public class ReopenRequestCommandHandler : IRequestHandler<ReopenRequestCommand, ApiResponse<string>>
{
    private readonly IRequestRepository _requestRepository;
    private readonly ICommentRepository _commentRepository;

    public ReopenRequestCommandHandler(IRequestRepository requestRepository, ICommentRepository commentRepository)
    {
        _requestRepository = requestRepository;
        _commentRepository = commentRepository;
    }

    public async Task<ApiResponse<string>> Handle(ReopenRequestCommand command, CancellationToken cancellationToken)
    {
        var request = await _requestRepository.GetRequestByIdAsync(command.RequestId, cancellationToken);
        if (request == null)
            return ApiResponse<string>.FailResponse("Request not found", 404);

        if (request.Status != RequestStatusEnum.Rejected)
            return ApiResponse<string>.FailResponse("Only rejected requests can be reopened", 409);

        if (request.EmployeeId != command.ReopenedBy)
            return ApiResponse<string>.FailResponse("You are not authorized to reopen this request", 403);

        // Fetch category (we can use Request.Category navigation if loaded, otherwise fetch)
        RequestCategory category = request.Category;
        if (category == null)
        {
            // fallback: load from repository
            category = await _requestRepository.GetRequestWithCategoryAsync(command.RequestId, cancellationToken) is { } r ? r.Category : null;
        }

        if (category == null)
            return ApiResponse<string>.FailResponse("Request category not found", 404);

        var newStatus = category.IsApprovalRequired ? RequestStatusEnum.PendingApproval : RequestStatusEnum.Open;

        // Update request
        request.Status = newStatus;
        request.RejectionReason = null;
        request.UpdatedOn = DateTime.UtcNow;

        // Add status history
        var history = new RequestStatusHistory
        {
            HistoryId = Guid.NewGuid(),
            RequestId = request.RequestId,
            OldStatus = RequestStatusEnum.Rejected,
            NewStatus = newStatus,
            ChangedBy = command.ReopenedBy,
            ChangedOn = DateTime.UtcNow,
            Remarks = "Request reopened by employee"
        };

        await _commentRepository.AddStatusHistoryAsync(history, cancellationToken);

        // Add comment
        var comment = new RequestComment
        {
            CommentId = Guid.NewGuid(),
            RequestId = request.RequestId,
            CommentBy = command.ReopenedBy,
            CommentText = command.ReopenReason,
            CommentType = CommentTypeEnum.Feedback, // General not defined; use Feedback as general textual comment
            CreatedOn = DateTime.UtcNow,
            IsDeleted = false
        };

        await _commentRepository.AddCommentAsync(comment, cancellationToken);

        // Persist both comment and history and update request
        _requestRepository.Update(request);
        await _commentRepository.SaveChangesAsync(cancellationToken);

        return ApiResponse<string>.SuccessResponse(null, "Request reopened successfully");
    }
}
