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

    public async Task<DashboardSummaryDto> GetSLASummaryAsync(DashboardQueryParams queryParams, CancellationToken cancellationToken = default)
    {
        var (where, parameters) = BuildWhereClause(queryParams);
        var slaSql = $@"
    SELECT
        SUM(CASE WHEN sr.IsEscalated = 1 THEN 1 ELSE 0 END) AS EscalatedCount,
        SUM(CASE WHEN sr.DueDate IS NOT NULL AND sr.Status NOT IN (7, 8) AND GETUTCDATE() > sr.DueDate THEN 1 ELSE 0 END) AS BreachedCount,
        SUM(CASE WHEN sr.DueDate IS NOT NULL AND sr.Status NOT IN (7, 8) AND GETUTCDATE() > DATEADD(HOUR,-2,sr.DueDate) AND GETUTCDATE() <= sr.DueDate THEN 1 ELSE 0 END) AS NearingBreachCount,
        SUM(CASE WHEN sr.DueDate IS NOT NULL AND sr.Status NOT IN (7, 8) AND GETUTCDATE() <= DATEADD(HOUR,-2,sr.DueDate) THEN 1 ELSE 0 END) AS WithinSLACount
    FROM ServiceRequests sr
    {where};";

        using var conn = _dapperContext.CreateConnection();
        var slaRow = await conn.QueryFirstAsync(new CommandDefinition(slaSql, parameters, cancellationToken: cancellationToken));

        return new DashboardSummaryDto
        {
            WithinSLACount = (int)slaRow.WithinSLACount,
            NearingBreachCount = (int)slaRow.NearingBreachCount,
            BreachedCount = (int)slaRow.BreachedCount,
            EscalatedCount = (int)slaRow.EscalatedCount
        };
    }

    /// <summary>
    /// Returns full dashboard summary by composing smaller aggregation queries.
    /// Executes all component queries in parallel for performance.
    /// </summary>
    public async Task<DashboardSummaryDto> GetSummaryAsync(
        DashboardQueryParams queryParams,
        CancellationToken cancellationToken = default)
    {
        if (queryParams is null) throw new ArgumentNullException(nameof(queryParams));

        var statusTask = GetStatusCountsAsync(queryParams, cancellationToken);
        var priorityTask = GetPriorityCountsAsync(queryParams, cancellationToken);
        var categoryTask = GetCategoryCountsAsync(queryParams, cancellationToken);
        var assignedUserTask = GetAssignedUserCountsAsync(queryParams, cancellationToken);

        await Task.WhenAll(statusTask, priorityTask, categoryTask, assignedUserTask);

        var statusCounts = await statusTask;
        var priorityCounts = await priorityTask;
        var categoryCounts = await categoryTask;
        var assignedUserCounts = await assignedUserTask;

        // Retrieve SLA / escalation totals in one query
        var (where, parameters) = BuildWhereClause(queryParams);
        var slaSql = $@"
            SELECT
            ISNULL(SUM(CASE WHEN sr.IsEscalated = 1 THEN 1 ELSE 0 END),0) AS EscalatedCount,
            ISNULL(SUM(CASE WHEN sr.DueDate IS NOT NULL AND sr.Status NOT IN (7, 8) AND GETUTCDATE() > sr.DueDate THEN 1 ELSE 0 END),0) AS BreachedCount,
            ISNULL(SUM(CASE WHEN sr.DueDate IS NOT NULL AND sr.Status NOT IN (7, 8) AND GETUTCDATE() > DATEADD(HOUR,-2,sr.DueDate) AND GETUTCDATE() <= sr.DueDate THEN 1 ELSE 0 END),0) AS NearingBreachCount,
            ISNULL(SUM(CASE WHEN sr.DueDate IS NOT NULL AND sr.Status NOT IN (7, 8) AND GETUTCDATE() <= DATEADD(HOUR,-2,sr.DueDate) THEN 1 ELSE 0 END),0) AS WithinSLACount
            FROM ServiceRequests sr
        {where};";

        using var conn = _dapperContext.CreateConnection();
        var slaRow = await conn.QueryFirstAsync(new CommandDefinition(slaSql, parameters, cancellationToken: cancellationToken));

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
            WithinSLACount = Convert.ToInt32(slaRow.WithinSLACount ?? 0),
            NearingBreachCount = (int)slaRow.NearingBreachCount,
            BreachedCount = (int)slaRow.BreachedCount,
            EscalatedCount = (int)slaRow.EscalatedCount,
            ByPriority = priorityCounts.ToList(),
            ByCategory = categoryCounts.ToList(),
            ByAssignedUser = assignedUserCounts.ToList()
        };
    }

    /// <summary>
    /// Returns counts per status as a dictionary keyed by int (enum value).
    /// </summary>
    public async Task<IReadOnlyDictionary<int, int>> GetStatusCountsAsync(
        DashboardQueryParams queryParams,
        CancellationToken cancellationToken = default)
    {
        if (queryParams is null) throw new ArgumentNullException(nameof(queryParams));

        var (where, parameters) = BuildWhereClause(queryParams);

        var sql = $@"
            SELECT sr.Status AS Status, COUNT(1) AS Count
            FROM ServiceRequests sr
            {where}
            GROUP BY sr.Status;";

        using var conn = _dapperContext.CreateConnection();
        var rows = await conn.QueryAsync(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));

        return rows.ToDictionary(
            row => (int)row.Status,
            row => (int)row.Count
        );
    }

    /// <summary>
    /// Returns counts grouped by priority.
    /// </summary>
    public async Task<IReadOnlyList<PriorityCountDto>> GetPriorityCountsAsync(
        DashboardQueryParams queryParams,
        CancellationToken cancellationToken = default)
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
        var rows = (await conn.QueryAsync(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken))).ToList();

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
    public async Task<IReadOnlyList<CategoryCountDto>> GetCategoryCountsAsync(
        DashboardQueryParams queryParams,
        CancellationToken cancellationToken = default)
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
        var rows = (await conn.QueryAsync(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken))).ToList();

        return rows.Select(row => new CategoryCountDto
        {
            CategoryName = row.CategoryName ?? string.Empty,
            Count = (int)row.Count
        }).ToList();
    }

    /// <summary>
    /// Returns request counts grouped by assigned support user.
    /// Only includes requests where AssignedTo is not null.
    /// </summary>
    public async Task<IReadOnlyList<AssignedUserCountDto>> GetAssignedUserCountsAsync(
        DashboardQueryParams queryParams,
        CancellationToken cancellationToken = default)
    {
        if (queryParams is null) throw new ArgumentNullException(nameof(queryParams));

        var (where, parameters) = BuildWhereClause(queryParams);

        // Extend WHERE to only include assigned requests
        var extendedWhere = string.IsNullOrEmpty(where)
            ? "WHERE sr.AssignedTo IS NOT NULL"
            : where + " AND sr.AssignedTo IS NOT NULL";

        var sql = $@"
            SELECT
                u.UserId   AS UserId,
                u.FullName AS FullName,
                COUNT(1)   AS Count
            FROM ServiceRequests sr
            INNER JOIN Users u ON sr.AssignedTo = u.UserId
            {extendedWhere}
            GROUP BY u.UserId, u.FullName
            HAVING COUNT(1) > 0
            ORDER BY COUNT(1) DESC;";

        using var conn = _dapperContext.CreateConnection();
        var rows = (await conn.QueryAsync(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken))).ToList();

        return rows.Select(row => new AssignedUserCountDto
        {
            UserId = (Guid)row.UserId,
            FullName = row.FullName ?? string.Empty,
            Count = (int)row.Count
        }).ToList();
    }

    /// <summary>
    /// Helper: builds WHERE clause and DynamicParameters from DashboardQueryParams.
    /// Role scoping: only one of EmployeeId, AssignedToUserId, ManagerId is set by the handler.
    /// </summary>
    private static (string whereClause, DynamicParameters parameters) BuildWhereClause(
        DashboardQueryParams queryParams)
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

        var whereClause = where.Count > 0
            ? "WHERE " + string.Join(" AND ", where)
            : string.Empty;

        return (whereClause, parameters);
    }
}