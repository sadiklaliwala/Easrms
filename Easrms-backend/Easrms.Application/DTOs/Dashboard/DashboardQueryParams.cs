// Easrms.Application/DTOs/Dashboard/DashboardQueryParams.cs

namespace Easrms.Application.DTOs.Dashboard;

/// <summary>
/// Role-scoping parameter object passed into all <c>IDashboardRepository</c> methods.
///
/// Dashboard has no pagination, no search, and no free-text filter.
/// Its only variable is WHO is asking — which determines which subset of
/// ServiceRequest records the aggregation runs against.
///
/// Scoping rules (applied by the handler before calling the repo):
/// ┌─────────────────┬──────────────────────────────────────────────────────┐
/// │ Role            │ Which param to set                                   │
/// ├─────────────────┼──────────────────────────────────────────────────────┤
/// │ Admin           │ All nulls — no scope filter, sees everything         │
/// │ Manager         │ Set ManagerId — sees only their team's requests      │
/// │ Employee        │ Set EmployeeId — sees only their own requests        │
/// │ Support User    │ Set AssignedToUserId — sees only their assigned work │
/// └─────────────────┴──────────────────────────────────────────────────────┘
///
/// The repository applies whichever non-null param it receives as a WHERE clause.
/// Only one scoping param should be non-null per call — validation enforced at handler level.
/// </summary>
public sealed class DashboardQueryParams
{
    // ─────────────────────────────────────────────────────────────────────────
    // ROLE SCOPE FILTERS
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Scope to requests created by a specific employee.
    /// Set this when the caller's role is Employee.
    /// Maps to ServiceRequest.EmployeeId.
    /// </summary>
    public Guid? EmployeeId { get; init; }

    /// <summary>
    /// Scope to requests created by employees who report to this manager.
    /// Set this when the caller's role is Manager.
    /// Maps to Users.ManagerId — the query joins Users on EmployeeId to check ManagerId.
    /// </summary>
    public Guid? ManagerId { get; init; }

    /// <summary>
    /// Scope to requests assigned to a specific support user.
    /// Set this when the caller's role is Support User.
    /// Maps to ServiceRequest.AssignedTo.
    /// </summary>
    public Guid? AssignedToUserId { get; init; }

    // ─────────────────────────────────────────────────────────────────────────
    // OPTIONAL DATE RANGE
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Restrict aggregations to requests created on or after this date (inclusive).
    /// Null means no lower date bound — all historical records included.
    /// Useful for time-boxed dashboard views (e.g. this month's summary).
    /// </summary>
    public DateTime? FromDate { get; init; }

    /// <summary>
    /// Restrict aggregations to requests created on or before this date (inclusive).
    /// Null means no upper date bound.
    /// </summary>
    public DateTime? ToDate { get; init; }

    // ─────────────────────────────────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns true if no role scope filter is set — indicating an Admin caller
    /// who sees all requests with no WHERE clause restriction on ownership.
    /// Used by the Dapper query builder to skip the scope JOIN entirely for performance.
    /// </summary>
    public bool IsGlobalScope => EmployeeId is null && ManagerId is null && AssignedToUserId is null;
}
