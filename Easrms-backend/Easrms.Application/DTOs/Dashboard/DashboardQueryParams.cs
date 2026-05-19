namespace Easrms.Application.DTOs.Dashboard;

/// <summary>
/// Encapsulates role-scoping filter parameters passed to <see cref="IDashboardRepository"/>.
///
/// Only ONE of the scope fields should be set at a time — the handler sets the relevant
/// field based on the caller's role. The repository builds the WHERE clause from whichever
/// field is non-null.
///
/// When all fields are null (Admin), the repository returns global unscoped aggregations.
/// </summary>
public class DashboardQueryParams
{
    /// <summary>
    /// Set when the caller is an Employee.
    /// Scopes all aggregations to requests created by this employee.
    /// </summary>
    public Guid? EmployeeId { get; set; }

    /// <summary>
    /// Set when the caller is a Manager.
    /// Scopes all aggregations to requests created by employees under this manager.
    /// </summary>
    public Guid? ManagerId { get; set; }

    /// <summary>
    /// Set when the caller is a Support User.
    /// Scopes all aggregations to requests assigned to this support user.
    /// </summary>
    public Guid? AssignedToUserId { get; set; }

    /// <summary>
    /// True when all scope fields are null — used by the repository to skip WHERE clause
    /// filtering and return global counts. Computed property for readability.
    /// </summary>
    /// 
    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public bool IsGlobalScope =>
        EmployeeId is null &&
        ManagerId is null &&
        AssignedToUserId is null;
}