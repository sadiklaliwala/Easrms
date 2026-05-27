using Easrms.Application.Interfaces.Email;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Common.Constants;
using Easrms.Common.Enums;
using Easrms.Common.Helpers;
using Easrms.Domain.Entities;
using MediatR;

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
    private readonly ICommentRepository _commentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;

    public CreateRequestCommandHandler(
        IRequestRepository requestRepository,
        ICategoryRepository categoryRepository,
        ICommentRepository commentRepository,
        IUserRepository userRepository,
        IEmailService emailService
    )
    {
        _requestRepository = requestRepository;
        _categoryRepository = categoryRepository;
        _commentRepository = commentRepository;
        _userRepository = userRepository;
        _emailService = emailService;

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
        Console.WriteLine(initialStatus);

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
        Console.WriteLine(entity.Status);
        await _requestRepository.AddAsync(entity, cancellationToken);
        Console.WriteLine(entity.Status);
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

        await _commentRepository.AddStatusHistoryAsync(history, cancellationToken);

        // 6. Single SaveChanges — both AddAsync and AddStatusHistoryAsync share
        //    the same DbContext instance, so one commit covers both inserts
        await _requestRepository.SaveChangesAsync(cancellationToken);

        // 6. Fire-and-forget email — runs after response is already on its way
        //    We capture what we need; do NOT pass DbContext or scoped services into the lambda.
        var employeeEmail = (await _userRepository.GetByIdAsync(request.CurrentUserId))?.Email;
        if (!string.IsNullOrWhiteSpace(employeeEmail))
        {
            var capturedEmail = employeeEmail;
            var capturedNumber = requestNumber;
            var capturedTitle = request.Title;

            //_ = Task.Run(async () =>
            //{
            await _emailService.SendRequestOpenedAsync(capturedEmail, capturedNumber, capturedTitle);
            //}, CancellationToken.None); // CancellationToken.None so it is NOT cancelled when the request ends
        }

        // 7. Return the human-readable number — controller wraps in 201
        return requestNumber;
    }
}