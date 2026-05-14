// Easrms.Infrastructure/Repositories/Interfaces/IDashboardRepository.cs


// Easrms.Infrastructure/Repositories/Interfaces/IDashboardRepository.cs

using Easrms.Application.DTOs.Dashboard;

namespace Easrms.Application.Interfaces.Repositories;

/// <summary>
/// Repository contract for all dashboard aggregation queries.
///
/// This repository is intentionally read-only — it contains no write methods,
/// no Update(), no SaveChangesAsync(). Dashboard data is always derived from
/// existing ServiceRequest records and never mutates state.
///
/// All methods use Dapper for performance. EF Core is not used here because
/// every method involves GROUP BY aggregations, COUNT operations, or multi-table
/// joins that would produce inefficient SQL if expressed as LINQ queries.
///
/// Role-scoping strategy:
/// The handler resolves the caller's role and passes the appropriate scope parameters
/// (e.g. EmployeeId for Employee, ManagerId for Manager) into the query params.
/// The repository applies the scope filter at the SQL level — it does not make
/// role decisions itself, keeping it role-agnostic and fully testable.
/// </summary>
public interface IDashboardRepository
{
    // ─────────────────────────────────────────────────────────────────────────
    // SUMMARY
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the full dashboard summary including status counts, priority breakdown,
    /// and category breakdown — all role-scoped via <see cref="DashboardQueryParams"/>.
    ///
    /// Internally executes multiple Dapper queries in parallel:
    ///   1. Status counts (TotalRequests + per-status counts) — single GROUP BY query
    ///   2. Priority breakdown — GROUP BY Priority
    ///   3. Category breakdown — GROUP BY CategoryId JOIN RequestCategory
    ///
    /// All three queries share the same WHERE clause built from <paramref name="queryParams"/>
    /// so role scoping is applied consistently across all aggregations.
    /// </summary>
    /// <param name="queryParams">
    /// Encapsulates role-scoping filters (EmployeeId, ManagerId, AssignedTo).
    /// The handler sets only the relevant scope param based on the caller's role.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>
    /// A fully populated <see cref="DashboardSummaryDto"/> with all counts and breakdowns.
    /// Counts default to 0 if no matching records exist — never null, never throws on empty data.
    /// </returns>
    Task<DashboardSummaryDto> GetSummaryAsync(
        DashboardQueryParams queryParams,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns only the per-status counts as a flat dictionary for lightweight polling.
    /// Used when the frontend needs to refresh status badge counts without reloading
    /// the full dashboard (e.g. after a status update action).
    ///
    /// Returns a dictionary keyed by status string value (e.g. "Open", "Assigned")
    /// with integer counts as values. Missing statuses default to 0 at the handler level.
    /// </summary>
    /// <param name="queryParams">Role-scoping filters — same params as GetSummaryAsync.</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>
    /// A <see cref="IReadOnlyDictionary{TKey,TValue}"/> of status → count.
    /// </returns>
    Task<IReadOnlyDictionary<string, int>> GetStatusCountsAsync(
        DashboardQueryParams queryParams,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns request counts grouped by priority for chart rendering.
    /// Separated from GetSummaryAsync to allow independent chart refresh
    /// without re-executing all aggregation queries.
    /// </summary>
    /// <param name="queryParams">Role-scoping filters.</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>
    /// A list of <see cref="PriorityCountDto"/> ordered by Priority value ascending.
    /// Returns empty list if no records match the scope.
    /// </returns>
    Task<IReadOnlyList<PriorityCountDto>> GetPriorityCountsAsync(
        DashboardQueryParams queryParams,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns request counts grouped by category for chart rendering.
    /// Only categories with at least one request in scope are included —
    /// zero-count categories are excluded to keep chart data clean.
    /// Ordered by Count DESC so the most active categories appear first.
    /// </summary>
    /// <param name="queryParams">Role-scoping filters.</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>
    /// A list of <see cref="CategoryCountDto"/> ordered by Count descending.
    /// Returns empty list if no records match the scope.
    /// </returns>
    Task<IReadOnlyList<CategoryCountDto>> GetCategoryCountsAsync(
        DashboardQueryParams queryParams,
        CancellationToken cancellationToken = default);
}