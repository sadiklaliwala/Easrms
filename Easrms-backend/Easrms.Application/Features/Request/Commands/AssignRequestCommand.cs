using Easrms.Application.Interfaces.Repositories;
using Easrms.Common.Constants;
using Easrms.Common.Enums;
using Easrms.Domain.Entities;
using MediatR;

namespace Easrms.Application.Features.Request.Commands;

/// <summary>
/// Admin assigns a support user to an Open or Approved request.
/// Only Admin can dispatch this command (enforced at controller level).
/// </summary>
public sealed class AssignRequestCommand : IRequest
{
    public Guid RequestId { get; init; }
    public Guid SupportUserId { get; init; }

    /// <summary>Admin's UserId extracted from JWT claims by the controller.</summary>
    public Guid CurrentUserId { get; init; }
}


/// <summary>
/// Step-by-step per HANDLER_REPO_REFERENCE_MAP:
///   1. GetRequestByIdAsync(requestId)
///      → null                             = 404
///      → Status not in (Open, Approved)   = 409 invalid transition
///   2. IUserRepository.ExistsAsync(supportUserId)
///      → false                            = 404 support user not found
///   3. entity.Status    = Assigned
///      entity.AssignedTo = supportUserId
///      entity.UpdatedOn  = UtcNow
///   4. IRequestRepository.Update(entity)
///   5. ICommentRepository.AddStatusHistoryAsync(history)
///      → OldStatus = previous status, NewStatus = Assigned
///   6. IRequestRepository.SaveChangesAsync()   ← single commit
/// </summary>
public sealed class AssignRequestCommandHandler(
    IRequestRepository requestRepository,
    ICommentRepository commentRepository,
    IUserRepository userRepository) : IRequestHandler<AssignRequestCommand>
{
    private readonly IRequestRepository _requestRepository = requestRepository;
    private readonly ICommentRepository _commentRepository = commentRepository;
    private readonly IUserRepository _userRepository = userRepository;

    public async Task Handle(
        AssignRequestCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Fetch and validate status
        var entity = await _requestRepository.GetRequestByIdAsync(
            request.RequestId,
            cancellationToken: cancellationToken)
            ?? throw new KeyNotFoundException(
                $"Service request with id '{request.RequestId}' was not found.");

        var assignableStatuses = new[]
            {
                RequestStatusEnum.Open,
                RequestStatusEnum.Approved
            };

        if (!assignableStatuses.Contains(entity.Status))
            throw new InvalidOperationException(
                $"Request '{entity.RequestNumber}' cannot be assigned in its current status '{entity.Status}'. " +
                $"Only Open or Approved requests can be assigned.");

        // 2. Verify the support user exists
        var supportUserExists = await _userRepository.ExistsAsync(
            request.SupportUserId,
            cancellationToken: cancellationToken);

        if (!supportUserExists)
            throw new KeyNotFoundException(
                $"Support user with id '{request.SupportUserId}' was not found.");

        // 3. Apply changes
        var oldStatus = entity.Status;
        entity.Status = Common.Enums.RequestStatusEnum.Assigned;
        entity.AssignedTo = request.SupportUserId;
        entity.UpdatedOn = DateTime.UtcNow;

        // 4. Mark dirty
        _requestRepository.Update(entity);

        // 5. Stage history entry
        var history = new RequestStatusHistory
        {
            RequestId = entity.RequestId,
            OldStatus = oldStatus,
            NewStatus = Common.Enums.RequestStatusEnum.Assigned,
            ChangedBy = request.CurrentUserId,
            ChangedOn = DateTime.UtcNow
        };

        await _commentRepository.AddStatusHistoryAsync(history, cancellationToken);

        // 6. Single commit — request update + history in one transaction
        await _requestRepository.SaveChangesAsync(cancellationToken);
    }
}