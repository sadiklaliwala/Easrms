using Easrms.Application.DTOs.Request;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Common.Constants;
using MediatR;

namespace Easrms.Application.Features.Request.Queries;

public sealed class GetAllRequestsQuery : IRequest<RequestListWithPaginationDto>
{
    // Pagination
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    // Filters
    public string? Search { get; init; }
    public string? Status { get; init; }
    public string? Priority { get; init; }
    public Guid? CategoryId { get; init; }
    // Injected by controller from JWT claims
    public Guid CurrentUserId { get; init; }
    public string CurrentUserRole { get; init; } = string.Empty;
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}

public sealed class GetAllRequestsQueryHandler(IRequestRepository requestRepository)
    : IRequestHandler<GetAllRequestsQuery, RequestListWithPaginationDto>
{
    private readonly IRequestRepository _requestRepository = requestRepository;

    public async Task<RequestListWithPaginationDto> Handle(
        GetAllRequestsQuery request,
        CancellationToken cancellationToken)
    {
        Guid? scopedEmployeeId = null;
        Guid? scopedAssignedTo = null;
        Guid? scopedManagerId = null;

        if (request.CurrentUserRole == RoleConstants.Employee)
            scopedEmployeeId = request.CurrentUserId;       // own requests only

        else if (request.CurrentUserRole == RoleConstants.SupportUser)
            scopedAssignedTo = request.CurrentUserId;       // assigned to them only

        else if (request.CurrentUserRole == RoleConstants.Manager)
            scopedManagerId = request.CurrentUserId;        // own + team requests

        // Admin → all three remain null → no WHERE filter → sees everything

        var requestQuery = new RequestQueryParams
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SearchTerm = request.Search,
            Status = request.Status,
            Priority = request.Priority,
            CategoryId = request.CategoryId,
            EmployeeId = scopedEmployeeId,
            AssignedTo = scopedAssignedTo,
            ManagerId = scopedManagerId,
            FromDate = request.FromDate,
            ToDate = request.ToDate,
        };

        return await _requestRepository.GetPagedRequestsAsync(
            requestQuery,
            cancellationToken: cancellationToken);
    }
}