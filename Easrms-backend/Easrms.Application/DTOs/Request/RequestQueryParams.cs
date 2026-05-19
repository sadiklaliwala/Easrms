namespace Easrms.Application.DTOs.Request;

/// <summary>
/// Strongly-typed query parameter object passed into <c>IRequestRepository.GetPagedRequestsAsync</c>.
/// Encapsulates all filtering, searching, sorting, and pagination inputs in a single model
/// instead of passing individual parameters — keeps the interface clean and extensible.
/// All filter properties are nullable; null means "no filter applied" for that field.
/// </summary>
public sealed class RequestQueryParams
{
    // ─────────────────────────────────────────────────────────────────────────
    // PAGINATION
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// The 1-based page number to return. Defaults to 1.
    /// </summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// Number of records per page. Defaults to 10. Maximum enforced at handler: 100.
    /// </summary>
    public int PageSize { get; init; } = 10;

    // ─────────────────────────────────────────────────────────────────────────
    // SEARCH
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Free-text search. Matches against RequestNumber (exact prefix) and Title (contains).
    /// Null or empty string disables search.
    /// </summary>
    public string? SearchTerm { get; init; }

    // ─────────────────────────────────────────────────────────────────────────
    // FILTERS
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Filter by request status (e.g. "Open", "Assigned", "Resolved").
    /// Null returns all statuses.
    /// </summary>
    public string? Status { get; init; }

    /// <summary>
    /// Filter by priority (e.g. "High", "Medium", "Low").
    /// Null returns all priorities.
    /// </summary>
    public string? Priority { get; init; }

    /// <summary>
    /// Filter by category ID. Null returns all categories.
    /// </summary>
    public Guid? CategoryId { get; init; }

    /// <summary>
    /// Filter by the employee who created the request.
    /// Used when an Employee role user is viewing their own requests.
    /// Null returns requests for all employees (Admin / Manager view).
    /// </summary>
    public Guid? EmployeeId { get; init; }

    /// <summary>
    /// Filter by the support user the request is assigned to.
    /// Used on the Support My Tasks screen and Admin Assignment screen.
    /// Null returns all assigned and unassigned requests.
    /// </summary>
    public Guid? AssignedTo { get; init; }

    /// <summary>
    /// Filter requests created on or after this date (inclusive).
    /// Null means no lower date bound.
    /// </summary>
    public DateTime? FromDate { get; init; }

    /// <summary>
    /// Filter requests created on or before this date (inclusive).
    /// Null means no upper date bound.
    /// </summary>
    public DateTime? ToDate { get; init; }

    // ─────────────────────────────────────────────────────────────────────────
    // SORT
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Column to sort by. Allowed values: "CreatedOn", "Priority", "Status", "RequestNumber".
    /// Defaults to "CreatedOn". Invalid values fall back to default at the Dapper query level.
    /// </summary>
    public string SortBy { get; init; } = "CreatedOn";

    /// <summary>
    /// Sort direction. <c>true</c> = ascending, <c>false</c> = descending.
    /// Defaults to <c>false</c> (newest first).
    /// </summary>
    public bool SortAscending { get; init; } = false;

    public Guid? ManagerId { get; set; }
}