// Easrms.Infrastructure/Repositories/Implementations/DashboardRepository.cs
using Dapper;
using Easrms.Application.DTOs.Dashboard;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Common.Enums;
using Easrms.Infrastructure.Data;

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

        var statusTask = GetStatusCountsAsync(queryParams, cancellationToken);
        var priorityTask = GetPriorityCountsAsync(queryParams, cancellationToken);
        var categoryTask = GetCategoryCountsAsync(queryParams, cancellationToken);

        await Task.WhenAll(statusTask, priorityTask, categoryTask);

        var statusCounts = await statusTask;
        var priorityCounts = await priorityTask;
        var categoryCounts = await categoryTask;

        // DB stores int — key the dictionary by enum int value
        return new DashboardSummaryDto
        {
            TotalRequests = statusCounts.Values.Sum(),
            OpenCount = statusCounts.GetValueOrDefault((int)RequestStatusEnum.Open),
            PendingApprovalCount = statusCounts.GetValueOrDefault((int)RequestStatusEnum.PendingApproval),
            ApprovedCount = statusCounts.GetValueOrDefault((int)RequestStatusEnum.Approved),
            RejectedCount = statusCounts.GetValueOrDefault((int)RequestStatusEnum.Rejected),
            AssignedCount = statusCounts.GetValueOrDefault((int)RequestStatusEnum.Assigned),
            InProgressCount = statusCounts.GetValueOrDefault((int)RequestStatusEnum.InProgress),
            ResolvedCount = statusCounts.GetValueOrDefault((int)RequestStatusEnum.Resolved),
            ClosedCount = statusCounts.GetValueOrDefault((int)RequestStatusEnum.Closed),
            ByPriority = priorityCounts.ToList(),
            ByCategory = categoryCounts.ToList()
        };
    }

    /// <summary>
    /// Returns counts per status as a dictionary keyed by int (enum value).
    /// </summary>
    public async Task<IReadOnlyDictionary<int, int>> GetStatusCountsAsync(DashboardQueryParams queryParams, CancellationToken cancellationToken = default)
    {
        if (queryParams is null) throw new ArgumentNullException(nameof(queryParams));

        var (where, parameters) = BuildWhereClause(queryParams);

        var sql = $@"
            SELECT sr.Status AS Status, COUNT(1) AS Count
            FROM ServiceRequests sr
            {where}
            GROUP BY sr.Status;";

        using var conn = _dapperContext.CreateConnection();
        var rows = await conn.QueryAsync(new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));

        // Status column is int in DB — cast directly, no string parsing
        return rows.ToDictionary(
            row => (int)row.Status,
            row => (int)row.Count
        );
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
        var rows = (await conn.QueryAsync(new CommandDefinition(sql, parameters, cancellationToken: cancellationToken))).ToList();

        return rows.Select(row =>
        {
            int priorityInt = (int)row.Priority;
            var priority = Enum.IsDefined(typeof(PriorityEnums), priorityInt)
                ? (PriorityEnums)priorityInt
                : PriorityEnums.Low;

            return new PriorityCountDto
            {
                Priority = priority,
                Count = (int)row.Count
            };
        }).ToList();
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
        var rows = (await conn.QueryAsync(new CommandDefinition(sql, parameters, cancellationToken: cancellationToken))).ToList();

        return rows.Select(row => new CategoryCountDto
        {
            CategoryName = row.CategoryName ?? string.Empty,
            Count = (int)row.Count
        }).ToList();
    }

    // Helper: build WHERE clause and DynamicParameters based on DashboardQueryParams
    private static (string whereClause, DynamicParameters parameters) BuildWhereClause(DashboardQueryParams queryParams)
    {
        var where = new List<string>();
        var parameters = new DynamicParameters();

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
            where.Add("EXISTS (SELECT 1 FROM Users u WHERE u.UserId = sr.EmployeeId AND u.ManagerId = @ManagerId)");
            parameters.Add("@ManagerId", queryParams.ManagerId.Value);
        }

        var whereClause = where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : string.Empty;
        return (whereClause, parameters);
    }
}