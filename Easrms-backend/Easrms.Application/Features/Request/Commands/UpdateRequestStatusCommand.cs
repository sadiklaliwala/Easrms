using Easrms.Application.Interfaces;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Common.Enums;
using Easrms.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Easrms.Application.Features.Request.Commands;

/// <summary>
/// Support user updates the status of their assigned request.
/// Valid transitions: Assigned → In Progress, In Progress → Resolved.
/// Remarks are mandatory when moving to Resolved.
/// </summary>
public sealed class UpdateRequestStatusCommand : IRequest
{
    public Guid RequestId { get; init; }
    public RequestStatusEnum NewStatus { get; init; }
    public string Remarks { get; init; } = string.Empty;

    /// <summary>Support user's UserId extracted from JWT claims by the controller.</summary>
    public Guid CurrentUserId { get; init; }
}

/// <summary>
/// Step-by-step per HANDLER_REPO_REFERENCE_MAP:
///   1. GetRequestByIdAsync(requestId)
///      → null = 404
///      → Validate transition:
///         Assigned    → In Progress  (Support User only)
///         In Progress → Resolved     (Support User only, Remarks mandatory)
///         any other pair = 409
///   2. entity.AssignedTo == currentUserId → 403 if mismatch
///   3. entity.Status    = newStatus
///      entity.UpdatedOn = UtcNow
///      If Resolved: entity.ResolvedOn = UtcNow
///   4. IRequestRepository.Update(entity)
///   5. ICommentRepository.AddStatusHistoryAsync(history)
///   6. IRequestRepository.SaveChangesAsync()
/// </summary>
public sealed class UpdateRequestStatusCommandHandler(
    IRequestRepository requestRepository,
    ICommentRepository commentRepository,
    IUserRepository userRepository,
    IEmailService emailService,
    ILogger<UpdateRequestStatusCommandHandler> logger
    ) : IRequestHandler<UpdateRequestStatusCommand>
{
    private readonly IRequestRepository _requestRepository = requestRepository;
    private readonly ICommentRepository _commentRepository = commentRepository;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IEmailService _emailService = emailService;
    private readonly ILogger<UpdateRequestStatusCommandHandler> _logger = logger;

    // Valid transitions: key = current status, value = expected new status
    private static readonly Dictionary<RequestStatusEnum, RequestStatusEnum> AllowedTransitions = new()
{
    { RequestStatusEnum.Assigned, RequestStatusEnum.InProgress },
    { RequestStatusEnum.InProgress, RequestStatusEnum.Resolved }
};

    public async Task Handle(
        UpdateRequestStatusCommand request,
        CancellationToken cancellationToken)
    {
        // 1a. Fetch entity
        var entity = await _requestRepository.GetRequestByIdAsync(
            request.RequestId,
            cancellationToken: cancellationToken)
            ?? throw new KeyNotFoundException(
                $"Service request with id '{request.RequestId}' was not found.");

        // 1b. Validate the transition is permitted
        if (!AllowedTransitions.TryGetValue(entity.Status, out var expectedNew)
            || expectedNew != request.NewStatus)
        {
            throw new InvalidOperationException(
                $"Status transition from '{entity.Status}' to '{request.NewStatus}' is not permitted. " +
                $"Allowed transitions: Assigned → In Progress, In Progress → Resolved.");
        }

        // 1c. Remarks mandatory when resolving
        if (request.NewStatus == RequestStatusEnum.Resolved
            && string.IsNullOrWhiteSpace(request.Remarks))
        {
            throw new InvalidOperationException(
                "Resolution remarks are mandatory when marking a request as Resolved.");
        }

        // 2. Support user scope — only the assigned user may update status
        if (entity.AssignedTo != request.CurrentUserId)
            throw new UnauthorizedAccessException(
                "You are not the assigned support user for this request.");

        // 3. Apply changes
        var oldStatus = entity.Status;
        entity.Status = request.NewStatus;
        entity.UpdatedOn = DateTime.UtcNow;

        if (request.NewStatus == RequestStatusEnum.Resolved)
            entity.ResolvedOn = DateTime.UtcNow;

        // 4. Mark dirty
        _requestRepository.Update(entity);

        // 5. Stage history entry
        var history = new RequestStatusHistory
        {
            HistoryId = Guid.NewGuid(),
            RequestId = entity.RequestId,
            OldStatus = oldStatus,
            NewStatus = request.NewStatus,
            ChangedBy = request.CurrentUserId,
            ChangedOn = DateTime.UtcNow,
            Remarks = request.Remarks
        };

        await _commentRepository.AddStatusHistoryAsync(history, cancellationToken);

        // 6. Single commit — request update + history in one transaction
        await _requestRepository.SaveChangesAsync(cancellationToken);


        // 6. Fire-and-forget email when status becomes Resolved
        //    Employee gets notified that they can now close the request.
        if (request.NewStatus == RequestStatusEnum.Resolved)
        {
            var employee = entity.Employee;
            if (!string.IsNullOrWhiteSpace(employee?.Email))
            {
                await _emailService.SendRequestResolvedAsync(
                    employee.Email,
                    entity.RequestNumber,
                    entity.Title);
            }
        }

        // SLA checks: compute nearing breach and breached based on DueDate and Category.SLAHours

        // SLA checks
        if (entity.DueDate.HasValue && entity.Category != null)
        {
            var now = DateTime.UtcNow;
            var slaHours = entity.Category.SLAHours;

            if (slaHours > 0)
            {
                var due = entity.DueDate.Value;
                var nearingThreshold = due - TimeSpan.FromHours(slaHours * 0.2);

                if (now > due)
                {
                    var recipients = new List<string?> { entity.AssignedUser?.Email, entity.Employee?.Email }
                        .Where(e => !string.IsNullOrWhiteSpace(e))
                        .Distinct()
                        .ToList();

                    foreach (var to in recipients)
                    {
                        await _emailService.SendSLABreachedAsync(to!, entity.RequestNumber, entity.Title);
                    }
                }
                else if (now > nearingThreshold && now <= due)
                {
                    var recipients = new List<string?> { entity.AssignedUser?.Email }
                        .Where(e => !string.IsNullOrWhiteSpace(e))
                        .Distinct()
                        .ToList();

                    foreach (var to in recipients)
                    {
                        await _emailService.SendSLANearingBreachAsync(to!, entity.RequestNumber, entity.Title);
                    }
                }
            }
        }
    }
}