using Easrms.Application.DTOs.Request;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Common.Constants;
using MediatR;

namespace Easrms.Application.Features.Request.Queries;

/// <summary>
/// Returns a paginated, filtered list of service requests.
/// Scope is role-driven — the handler applies the correct data boundary
/// based on CurrentUserRole and CurrentUserId.
///
/// Role → scope applied by handler:
///   Employee     → only requests where EmployeeId == CurrentUserId
///   Support User → only requests where AssignedTo  == CurrentUserId
///   Admin        → all requests (no filter)
///   Manager      → all requests (no filter; managers see team via approval queue)
/// </summary>
public sealed class GetAllRequestsQuery : IRequest<RequestListWithPaginationDto>
{
    // Pagination
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;

    // Filters
    public string? Search { get; init; }   // request number or title
    public string? Status { get; init; }
    public string? Priority { get; init; }
    public Guid? CategoryId { get; init; }

    // Injected by controller from JWT claims
    public Guid CurrentUserId { get; init; }
    public string CurrentUserRole { get; init; } = string.Empty;

    public string RoleName { get; set; } = string.Empty;
}


/// <summary>
/// Step-by-step per HANDLER_REPO_REFERENCE_MAP:
///   1. Build queryParams — set EmployeeId if Employee, AssignedTo if Support User,
///      no scope filter for Admin or Manager
///   2. IRequestRepository.GetPagedRequestsAsync(queryParams)
///      → returns RequestListWithPaginationDto directly (Dapper projection)
/// </summary>
public sealed class GetAllRequestsQueryHandler(IRequestRepository requestRepository) : IRequestHandler<GetAllRequestsQuery, RequestListWithPaginationDto>
{
    private readonly IRequestRepository _requestRepository = requestRepository;

    public async Task<RequestListWithPaginationDto> Handle(
        GetAllRequestsQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Build scoped query params based on caller's role
        Guid? scopedEmployeeId = null;
        Guid? scopedAssignedTo = null;

        if (request.CurrentUserRole == RoleConstants.Employee)
            scopedEmployeeId = request.CurrentUserId;
        else if (request.CurrentUserRole == RoleConstants.SupportUser)
            scopedAssignedTo = request.CurrentUserId;
        // Admin and Manager: no scope filter — both fields remain null

        var requestQuery = new RequestQueryParams()
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SearchTerm = request.Search,
            Status = request.Status,
            Priority = request.Priority,
            CategoryId = request.CategoryId,
            EmployeeId = scopedEmployeeId,
            AssignedTo = scopedAssignedTo,
        };

        // 2. Delegate to repository — Dapper handles projection + pagination
        return await _requestRepository.GetPagedRequestsAsync(
            requestQuery,
            cancellationToken: cancellationToken);
    }
}