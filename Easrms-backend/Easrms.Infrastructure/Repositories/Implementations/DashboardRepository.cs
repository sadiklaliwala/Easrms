// Easrms.Infrastructure/Repositories/Implementations/DashboardRepository.cs
using Dapper;
using Easrms.Application.DTOs.Dashboard;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Infrastructure.Data;
using Microsoft.Data.SqlClient;

namespace Easrms.Infrastructure.Repositories.Implementations;

/// <summary>
/// Dapper-based repository for dashboard aggregations. All methods are read-only
/// and execute parameterized SQL aggregations for performance.
/// </summary>
public class DashboardRepository : IDashboardRepository
{
    private readonly DapperContext _dapperContext;

    public DashboardRepository(DapperContext dapperContext)
    {
        _dapperContext = dapperContext ?? throw new ArgumentNullException(nameof(dapperContext));
    }

    /// <summary>
    /// Returns full dashboard summary by composing smaller aggregation queries.
    /// Executes the component queries in parallel for performance.
    /// </summary>
    public async Task<DashboardSummaryDto> GetSummaryAsync(DashboardQueryParams queryParams, CancellationToken cancellationToken = default)
    {
        if (queryParams is null) throw new ArgumentNullException(nameof(queryParams));

        // Run component queries in parallel
        var statusTask = GetStatusCountsAsync(queryParams, cancellationToken);
        var priorityTask = GetPriorityCountsAsync(queryParams, cancellationToken);
        var categoryTask = GetCategoryCountsAsync(queryParams, cancellationToken);

        await Task.WhenAll(statusTask, priorityTask, categoryTask);

        var statusCounts = await statusTask;
        var priorityCounts = await priorityTask;
        var categoryCounts = await categoryTask;

        // Map status counts to DashboardSummaryDto fields using known status names.
        var summary = new DashboardSummaryDto
        {
            TotalRequests = statusCounts.Values.Sum(),
            OpenCount = statusCounts.TryGetValue(Common.Constants.StatusConstants.Open, out var v1) ? v1 : 0,
            PendingApprovalCount = statusCounts.TryGetValue(Common.Constants.StatusConstants.PendingApproval, out var v2) ? v2 : 0,
            ApprovedCount = statusCounts.TryGetValue(Common.Constants.StatusConstants.Approved, out var v3) ? v3 : 0,
            RejectedCount = statusCounts.TryGetValue(Common.Constants.StatusConstants.Rejected, out var v4) ? v4 : 0,
            AssignedCount = statusCounts.TryGetValue(Common.Constants.StatusConstants.Assigned, out var v5) ? v5 : 0,
            InProgressCount = statusCounts.TryGetValue(Common.Constants.StatusConstants.InProgress, out var v6) ? v6 : 0,
            ResolvedCount = statusCounts.TryGetValue(Common.Constants.StatusConstants.Resolved, out var v7) ? v7 : 0,
            ClosedCount = statusCounts.TryGetValue(Common.Constants.StatusConstants.Closed, out var v8) ? v8 : 0,
            ByPriority = priorityCounts.ToList(),
            ByCategory = categoryCounts.ToList()
        };

        return summary;
    }

    /// <summary>
    /// Returns counts per status as a dictionary.
    /// </summary>
    public async Task<IReadOnlyDictionary<string, int>> GetStatusCountsAsync(DashboardQueryParams queryParams, CancellationToken cancellationToken = default)
    {
        if (queryParams is null) throw new ArgumentNullException(nameof(queryParams));

        var (where, parameters) = BuildWhereClause(queryParams);

        var sql = $@"
SELECT sr.Status AS Status, COUNT(1) AS Count
FROM ServiceRequests sr
{where}
GROUP BY sr.Status;";

        using var conn = _dapperContext.CreateConnection();
        var rows = await conn.QueryAsync<(string Status, int Count)>(new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));

        var dict = rows.ToDictionary(r => r.Status ?? string.Empty, r => r.Count, StringComparer.OrdinalIgnoreCase);
        return dict;
    }

    /// <summary>
    /// Returns counts grouped by priority.
    /// </summary>
    public async Task<IReadOnlyList<PriorityCountDto>> GetPriorityCountsAsync(DashboardQueryParams queryParams, CancellationToken cancellationToken = default)
    {
        if (queryParams is null) throw new ArgumentNullException(nameof(queryParams));

        var (where, parameters) = BuildWhereClause(queryParams);

        var sql = $@"
SELECT sr.Priority AS Priority, COUNT(1) AS Count
FROM ServiceRequests sr
{where}
GROUP BY sr.Priority
ORDER BY sr.Priority ASC;";

        using var conn = _dapperContext.CreateConnection();
        var rows = await conn.QueryAsync(new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));

        var list = new List<PriorityCountDto>();
        foreach (var row in rows)
        {
            int priorityVal = 0;
            try
            {
                var p = (row.Priority as object)?.ToString();
                if (!string.IsNullOrEmpty(p)) int.TryParse(p, out priorityVal);
            }
            catch { }

            list.Add(new PriorityCountDto
            {
                Priority = priorityVal,
                Count = row.Count
            });
        }

        return list;
    }

    /// <summary>
    /// Returns counts grouped by category name ordered by count desc.
    /// </summary>
    public async Task<IReadOnlyList<CategoryCountDto>> GetCategoryCountsAsync(DashboardQueryParams queryParams, CancellationToken cancellationToken = default)
    {
        if (queryParams is null) throw new ArgumentNullException(nameof(queryParams));

        var (where, parameters) = BuildWhereClause(queryParams);

        var sql = $@"
SELECT rc.CategoryName AS CategoryName, COUNT(1) AS Count
FROM ServiceRequests sr
INNER JOIN RequestCategories rc ON sr.CategoryId = rc.CategoryId
{where}
GROUP BY rc.CategoryName
HAVING COUNT(1) > 0
ORDER BY COUNT(1) DESC;";

        using var conn = _dapperContext.CreateConnection();
        var rows = await conn.QueryAsync(new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));

        var list = new List<CategoryCountDto>();
        foreach (var row in rows)
        {
            list.Add(new CategoryCountDto
            {
                CategoryName = row.CategoryName ?? string.Empty,
                Count = row.Count
            });
        }

        return list;
    }

    // Helper: build WHERE clause and DynamicParameters based on DashboardQueryParams
    private static (string whereClause, DynamicParameters parameters) BuildWhereClause(DashboardQueryParams queryParams)
    {
        var where = new List<string>();
        var parameters = new DynamicParameters();

        // Apply date filters
        if (queryParams.FromDate.HasValue)
        {
            where.Add("sr.CreatedOn >= @FromDate");
            parameters.Add("@FromDate", queryParams.FromDate.Value.ToUniversalTime());
        }

        if (queryParams.ToDate.HasValue)
        {
            where.Add("sr.CreatedOn <= @ToDate");
            parameters.Add("@ToDate", queryParams.ToDate.Value.ToUniversalTime());
        }

        // Role scoping: only one of EmployeeId, ManagerId, AssignedToUserId should be set by handler
        if (queryParams.EmployeeId.HasValue)
        {
            where.Add("sr.EmployeeId = @EmployeeId");
            parameters.Add("@EmployeeId", queryParams.EmployeeId.Value);
        }
        else if (queryParams.AssignedToUserId.HasValue)
        {
            where.Add("sr.AssignedTo = @AssignedToUserId");
            parameters.Add("@AssignedToUserId", queryParams.AssignedToUserId.Value);
        }
        else if (queryParams.ManagerId.HasValue)
        {
            // Manager scoping requires joining users — handled by adding EXISTS join condition
            where.Add("EXISTS (SELECT 1 FROM Users u WHERE u.UserId = sr.EmployeeId AND u.ManagerId = @ManagerId)");
            parameters.Add("@ManagerId", queryParams.ManagerId.Value);
        }

        var whereClause = where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : string.Empty;
        return (whereClause, parameters);
    }
}

