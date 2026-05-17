using Easrms.Application.Interfaces.Repositories;
using Easrms.Common.Constants;
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
    public string Priority { get; init; } = string.Empty;

    /// <summary>EmployeeId extracted from JWT claims by the controller.</summary>
    public Guid CurrentUserId { get; init; }


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

    public CreateRequestCommandHandler(
        IRequestRepository requestRepository,
        ICategoryRepository categoryRepository,
        ICommentRepository commentRepository)
    {
        _requestRepository = requestRepository;
        _categoryRepository = categoryRepository;
        _commentRepository = commentRepository;
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

        // 2. Generate a unique request number
        string requestNumber;
        do
        {
            requestNumber = RequestNumberHelper.Generate();
        }
        while (await _requestRepository.IsRequestNumberExistsAsync(requestNumber, cancellationToken));

        // 3. Determine initial status from category approval flag
        var initialStatus = category.IsApprovalRequired
            ? StatusConstants.PendingApproval
            : StatusConstants.Open;

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
            CreatedOn = DateTime.UtcNow
        };

        await _requestRepository.AddAsync(entity, cancellationToken);

        // 5. Seed the first history entry — OldStatus is null on creation
        var history = new RequestStatusHistory
        {
            HistoryId = Guid.NewGuid(),
            RequestId = entity.RequestId,
            OldStatus = null,
            NewStatus = initialStatus,
            ChangedBy = request.CurrentUserId,
            ChangedOn = DateTime.UtcNow,
            Remarks = "Request created."
        };

        await _commentRepository.AddStatusHistoryAsync(history, cancellationToken);

        // 6. Single SaveChanges — both AddAsync and AddStatusHistoryAsync share
        //    the same DbContext instance, so one commit covers both inserts
        await _requestRepository.SaveChangesAsync(cancellationToken);

        // 7. Return the human-readable number — controller wraps in 201
        return requestNumber;
    }
}