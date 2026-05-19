using Easrms.Application.Interfaces.Repositories;
using Easrms.Common.Constants;
using Easrms.Common.Enums;
using Easrms.Domain.Entities;
using MediatR;

namespace Easrms.Application.Features.Request.Commands;

/// <summary>
/// Closes a Resolved service request.
/// Can be dispatched by Admin or by the Employee who owns the request.
/// Authorization is validated inside the handler.
/// </summary>
public sealed class CloseRequestCommand : IRequest
{
    public Guid RequestId { get; init; }

    /// <summary>UserId extracted from JWT claims by the controller.</summary>
    public Guid CurrentUserId { get; init; }
    public string CurrentUserRole { get; init; } = string.Empty;
}


/// <summary>
/// Step-by-step per HANDLER_REPO_REFERENCE_MAP:
///   1. GetRequestByIdAsync(requestId)
///      → null                = 404
///      → Status != Resolved  = 409 can only close resolved requests
///      → caller not Admin and caller != entity.EmployeeId = 403
///   2. entity.Status    = Closed
///      entity.ClosedOn  = UtcNow
///      entity.ClosedBy  = currentUserId
///      entity.UpdatedOn = UtcNow
///   3. IRequestRepository.Update(entity)
///   4. ICommentRepository.AddStatusHistoryAsync(history)
///      → OldStatus = Resolved, NewStatus = Closed
///   5. IRequestRepository.SaveChangesAsync()
/// </summary>
public sealed class CloseRequestCommandHandler(
    IRequestRepository requestRepository,
    ICommentRepository commentRepository) : IRequestHandler<CloseRequestCommand>
{
    private readonly IRequestRepository _requestRepository = requestRepository;
    private readonly ICommentRepository _commentRepository = commentRepository;

    public async Task Handle(
        CloseRequestCommand request,
        CancellationToken cancellationToken)
    {
        // 1a. Fetch entity
        var entity = await _requestRepository.GetRequestByIdAsync(
            request.RequestId,
            cancellationToken: cancellationToken)
            ?? throw new KeyNotFoundException(
                $"Service request with id '{request.RequestId}' was not found.");

        // 1b. Only Resolved requests can be closed
        if (entity.Status != RequestStatusEnum.Resolved)
            throw new InvalidOperationException(
                $"Request '{entity.RequestNumber}' cannot be closed because its current status " +
                $"is '{entity.Status}'. Only Resolved requests can be closed.");

        // 1c. Authorization — Admin can close any request; Employee can only close their own
        var isAdmin = request.CurrentUserRole == RoleConstants.Admin;
        var isOwner = entity.EmployeeId == request.CurrentUserId;

        if (!isAdmin && !isOwner)
            throw new UnauthorizedAccessException(
                "You do not have permission to close this request.");

        // 2. Apply changes
        entity.Status = RequestStatusEnum.Closed;
        entity.ClosedOn = DateTime.UtcNow;
        entity.ClosedBy = request.CurrentUserId;
        entity.UpdatedOn = DateTime.UtcNow;

        // 3. Mark dirty
        _requestRepository.Update(entity);

        // 4. Stage history entry
        var history = new RequestStatusHistory
        {
            HistoryId = Guid.NewGuid(),
            RequestId = entity.RequestId,
            OldStatus = RequestStatusEnum.Resolved,
            NewStatus = RequestStatusEnum.Closed,
            ChangedBy = request.CurrentUserId,
            ChangedOn = DateTime.UtcNow
        };

        await _commentRepository.AddStatusHistoryAsync(history, cancellationToken);

        // 5. Single commit — request update + history in one transaction
        await _requestRepository.SaveChangesAsync(cancellationToken);
    }
}