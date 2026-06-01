using Easrms.Application.Interfaces.Email;
using Easrms.Application.Interfaces.Notifications;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Common.Constants;
using Easrms.Common.Enums;
using Easrms.Common.Helpers;
using Easrms.Domain.Entities;
using MediatR;
using INotificationPublisher = Easrms.Application.Interfaces.Notifications.INotificationPublisher;

namespace Easrms.Application.Features.Request.Commands;

/// <summary>
/// Creates a new service request for the authenticated employee.
/// EmployeeId is always the authenticated user — set by the controller from JWT claims.
/// Initial status is determined by the category's IsApprovalRequired flag.
/// </summary>
public sealed class CreateRequestCommand : IRequest<string>
{
    public Guid CategoryId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public PriorityEnums Priority { get; init; } = PriorityEnums.Low;

    /// <summary>EmployeeId extracted from JWT claims by the controller.</summary>
    public Guid CurrentUserId { get; init; }

    public string? AttachmentUrl { get; init; }

}

/// <summary>
/// Step-by-step per HANDLER_REPO_REFERENCE_MAP:
///   1. ICategoryRepository.GetByIdAsync(categoryId)
///      → null     = 404 category not found
///      → inactive = 409 category not active
///   2. RequestNumberHelper.Generate() → loop IsRequestNumberExistsAsync() until unique
///   3. Construct ServiceRequest entity
///      → Status = category.IsApprovalRequired ? StatusConstants.PendingApproval : StatusConstants.Open
///   4. IRequestRepository.AddAsync(entity)
///   5. ICommentRepository.AddStatusHistoryAsync(history)
///      → OldStatus = null, NewStatus = entity.Status, ChangedBy = currentUserId
///   6. IRequestRepository.SaveChangesAsync()   ← single commit for both inserts
///   7. Return RequestNumber (201 set by controller)
/// </summary>
public sealed class CreateRequestCommandHandler : IRequestHandler<CreateRequestCommand, string>
{
    private readonly IRequestRepository _requestRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICommentRepository _comment_repository;
    private readonly IUserRepository _user_repository;
    private readonly IEmailService _emailService;
    private readonly INotificationPublisher _notificationPublisher;

    public CreateRequestCommandHandler(
        IRequestRepository requestRepository,
        ICategoryRepository categoryRepository,
        ICommentRepository commentRepository,
        IUserRepository userRepository,
        IEmailService emailService,
        INotificationPublisher notificationPublisher
    )
    {
        _requestRepository = requestRepository;
        _categoryRepository = categoryRepository;
        _comment_repository = commentRepository;
        _user_repository = userRepository;
        _emailService = emailService;
        _notificationPublisher = notificationPublisher;

    }

    public async Task<string> Handle(
        CreateRequestCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Validate category
        var category = await _categoryRepository.GetByIdAsync(
            request.CategoryId,
            cancellationToken: cancellationToken)
            ?? throw new KeyNotFoundException(
                $"Category with id '{request.CategoryId}' was not found.");

        if (!category.IsActive)
            throw new InvalidOperationException(
                $"Category '{category.CategoryName}' is not active and cannot accept new requests.");

        var createdOn = DateTime.UtcNow;
        var dueDate = createdOn.AddHours(category.SLAHours);

        // 2. Generate a unique request number
        string requestNumber;
        do
        {
            requestNumber = RequestNumberHelper.Generate();
        }
        while (await _requestRepository.IsRequestNumberExistsAsync(requestNumber, cancellationToken));

        // 3. Determine initial status from category approval flag
        var initialStatus = category.IsApprovalRequired
    ? RequestStatusEnum.PendingApproval
    : RequestStatusEnum.Open;

        // 4. Build entity
        var entity = new ServiceRequest
        {
            RequestId = Guid.NewGuid(),
            RequestNumber = requestNumber,
            EmployeeId = request.CurrentUserId,
            CategoryId = request.CategoryId,
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            Status = initialStatus,
            CreatedOn = createdOn,
            DueDate = dueDate,
            AttachmentUrl = request.AttachmentUrl
        };

        await _requestRepository.AddAsync(entity, cancellationToken);

        // 5. Seed the first history entry — OldStatus is null on creation
        var history = new RequestStatusHistory
        {
            HistoryId = Guid.NewGuid(),
            RequestId = entity.RequestId,
            OldStatus = null,
            NewStatus = RequestStatusEnum.Open,
            ChangedBy = request.CurrentUserId,
            ChangedOn = DateTime.UtcNow,
            Remarks = "Request created."
        };

        await _comment_repository.AddStatusHistoryAsync(history, cancellationToken);

        // 6. Single SaveChanges — both AddAsync and AddStatusHistoryAsync share
        //    the same DbContext instance, so one commit covers both inserts
        await _requestRepository.SaveChangesAsync(cancellationToken);

        // fetch employee once for email and notification
        var employee = await _user_repository.GetByIdAsync(request.CurrentUserId, cancellationToken: cancellationToken);
        var employeeName = employee?.FullName;
        var employeeEmail = employee?.Email;

        // SignalR notifications via abstraction
        if (entity.Status == RequestStatusEnum.PendingApproval)
        {
            await _notificationPublisher.PublishToGroupAsync(RoleConstants.Manager, SignalREvents.NewRequestPendingApproval, new { RequestId = entity.RequestId, RequestNumber = entity.RequestNumber, Title = entity.Title, EmployeeName = employeeName }, cancellationToken);
        }
        else if (entity.Status == RequestStatusEnum.Open)
        {
            await _notificationPublisher.PublishToGroupAsync(RoleConstants.Admin, SignalREvents.NewRequestOpen, new { RequestId = entity.RequestId, RequestNumber = entity.RequestNumber, Title = entity.Title }, cancellationToken);
        }

        // 6. Fire-and-forget email — runs after response is already on its way
        if (!string.IsNullOrWhiteSpace(employeeEmail))
        {
            await _emailService.SendRequestOpenedAsync(employeeEmail!, requestNumber, request.Title);
        }

        // 7. Return the human-readable number — controller wraps in 201
        return requestNumber;
    }
}