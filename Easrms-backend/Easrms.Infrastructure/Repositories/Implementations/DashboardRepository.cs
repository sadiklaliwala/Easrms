// DashboardRepository.cs
using Dapper;
using Easrms.Application.DTOs.Dashboard;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Common.Enums;
using Easrms.Infrastructure.Data;

namespace Easrms.Infrastructure.Repositories.Implementations;

public class DashboardRepository : IDashboardRepository
{
    private readonly DapperContext _dapperContext;

    public DashboardRepository(DapperContext dapperContext)
    {
        _dapperContext = dapperContext ?? throw new ArgumentNullException(nameof(dapperContext));
    }

    // ─── Private typed class to avoid dynamic casing issues with PostgreSQL ───
    private class SlaRow
    {
        public long WithinSLACount { get; set; }
        public long NearingBreachCount { get; set; }
        public long BreachedCount { get; set; }
        public long EscalatedCount { get; set; }
    }

    private class StatusRow
    {
        public int Status { get; set; }
        public long Count { get; set; }
    }

    private class PriorityRow
    {
        public int Priority { get; set; }
        public long Count { get; set; }
    }

    private class CategoryRow
    {
        public string CategoryName { get; set; } = string.Empty;
        public long Count { get; set; }
    }

    private class AssignedUserRow
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public long Count { get; set; }
    }

    // ─── SLA Summary Only ─────────────────────────────────────────────────────
    public async Task<DashboardSummaryDto> GetSLASummaryAsync(
        DashboardQueryParams queryParams,
        CancellationToken cancellationToken = default)
    {
        var (where, parameters) = BuildWhereClause(queryParams);

        var slaSql = $@"
            SELECT
                COALESCE(SUM(CASE WHEN sr.is_escalated = TRUE THEN 1 ELSE 0 END), 0)                                                                                                              AS EscalatedCount,
                COALESCE(SUM(CASE WHEN sr.due_date IS NOT NULL AND sr.status NOT IN (7, 8) AND NOW() > sr.due_date THEN 1 ELSE 0 END), 0)                                                          AS BreachedCount,
                COALESCE(SUM(CASE WHEN sr.due_date IS NOT NULL AND sr.status NOT IN (7, 8) AND NOW() > (sr.due_date - INTERVAL '2 hours') AND NOW() <= sr.due_date THEN 1 ELSE 0 END), 0)          AS NearingBreachCount,
                COALESCE(SUM(CASE WHEN sr.due_date IS NOT NULL AND sr.status NOT IN (7, 8) AND NOW() <= (sr.due_date - INTERVAL '2 hours') THEN 1 ELSE 0 END), 0)                                  AS WithinSLACount
            FROM service_requests sr
            {where};";

        using var conn = _dapperContext.CreateConnection();
        var slaRow = await conn.QueryFirstOrDefaultAsync<SlaRow>(
            new CommandDefinition(slaSql, parameters, cancellationToken: cancellationToken))
            ?? new SlaRow();

        return new DashboardSummaryDto
        {
            WithinSLACount = (int)slaRow.WithinSLACount,
            NearingBreachCount = (int)slaRow.NearingBreachCount,
            BreachedCount = (int)slaRow.BreachedCount,
            EscalatedCount = (int)slaRow.EscalatedCount
        };
    }

    // ─── Full Dashboard Summary ───────────────────────────────────────────────
    public async Task<DashboardSummaryDto> GetSummaryAsync(
        DashboardQueryParams queryParams,
        CancellationToken cancellationToken = default)
    {
        if (queryParams is null) throw new ArgumentNullException(nameof(queryParams));

        // Run all sub-queries in parallel
        var statusTask = GetStatusCountsAsync(queryParams, cancellationToken);
        var priorityTask = GetPriorityCountsAsync(queryParams, cancellationToken);
        var categoryTask = GetCategoryCountsAsync(queryParams, cancellationToken);
        var assignedUserTask = GetAssignedUserCountsAsync(queryParams, cancellationToken);

        await Task.WhenAll(statusTask, priorityTask, categoryTask, assignedUserTask);

        var statusCounts = await statusTask;
        var priorityCounts = await priorityTask;
        var categoryCounts = await categoryTask;
        var assignedUserCounts = await assignedUserTask;

        // SLA totals
        var (where, parameters) = BuildWhereClause(queryParams);
        var slaSql = $@"
            SELECT
                COALESCE(SUM(CASE WHEN sr.is_escalated = TRUE THEN 1 ELSE 0 END), 0)                                                                                                              AS EscalatedCount,
                COALESCE(SUM(CASE WHEN sr.due_date IS NOT NULL AND sr.status NOT IN (7, 8) AND NOW() > sr.due_date THEN 1 ELSE 0 END), 0)                                                          AS BreachedCount,
                COALESCE(SUM(CASE WHEN sr.due_date IS NOT NULL AND sr.status NOT IN (7, 8) AND NOW() > (sr.due_date - INTERVAL '2 hours') AND NOW() <= sr.due_date THEN 1 ELSE 0 END), 0)          AS NearingBreachCount,
                COALESCE(SUM(CASE WHEN sr.due_date IS NOT NULL AND sr.status NOT IN (7, 8) AND NOW() <= (sr.due_date - INTERVAL '2 hours') THEN 1 ELSE 0 END), 0)                                  AS WithinSLACount
            FROM service_requests sr
            {where};";

        using var conn = _dapperContext.CreateConnection();
        var slaRow = await conn.QueryFirstOrDefaultAsync<SlaRow>(
            new CommandDefinition(slaSql, parameters, cancellationToken: cancellationToken))
            ?? new SlaRow();

        int? managedEmployeesCount = null;
        List<Easrms.Application.DTOs.User.UserListDto> managedEmployees = new();

        if (queryParams.ManagerId.HasValue)
        {
            var empSql = @"
                SELECT u.user_id AS UserId, u.full_name AS FullName, u.email AS Email, r.role_name AS RoleName, u.is_active AS IsActive, u.created_on AS CreatedOn
                FROM users u
                LEFT JOIN roles r ON u.role_id = r.role_id
                WHERE u.manager_id = @ManagerId AND u.is_deleted = FALSE;";
            
            var emps = (await conn.QueryAsync<Easrms.Application.DTOs.User.UserListDto>(
                new CommandDefinition(empSql, new { ManagerId = queryParams.ManagerId.Value }, cancellationToken: cancellationToken))).ToList();
            
            managedEmployees = emps;
            managedEmployeesCount = emps.Count;
        }

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
            WithinSLACount = (int)slaRow.WithinSLACount,
            NearingBreachCount = (int)slaRow.NearingBreachCount,
            BreachedCount = (int)slaRow.BreachedCount,
            EscalatedCount = (int)slaRow.EscalatedCount,
            ByPriority = priorityCounts.ToList(),
            ByCategory = categoryCounts.ToList(),
            ByAssignedUser = assignedUserCounts.ToList(),
            ManagedEmployeesCount = managedEmployeesCount,
            ManagedEmployees = managedEmployees
        };
    }

    // ─── Status Counts ────────────────────────────────────────────────────────
    public async Task<IReadOnlyDictionary<int, int>> GetStatusCountsAsync(
        DashboardQueryParams queryParams,
        CancellationToken cancellationToken = default)
    {
        if (queryParams is null) throw new ArgumentNullException(nameof(queryParams));

        var (where, parameters) = BuildWhereClause(queryParams);

        var sql = $@"
            SELECT sr.status AS Status, COUNT(1) AS Count
            FROM service_requests sr
            {where}
            GROUP BY sr.status;";

        using var conn = _dapperContext.CreateConnection();
        var rows = await conn.QueryAsync<StatusRow>(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));

        return rows.ToDictionary(
            row => row.Status,
            row => (int)row.Count
        );
    }

    // ─── Priority Counts ──────────────────────────────────────────────────────
    public async Task<IReadOnlyList<PriorityCountDto>> GetPriorityCountsAsync(
        DashboardQueryParams queryParams,
        CancellationToken cancellationToken = default)
    {
        if (queryParams is null) throw new ArgumentNullException(nameof(queryParams));

        var (where, parameters) = BuildWhereClause(queryParams);

        var sql = $@"
            SELECT sr.priority AS Priority, COUNT(1) AS Count
            FROM service_requests sr
            {where}
            GROUP BY sr.priority
            ORDER BY sr.priority ASC;";

        using var conn = _dapperContext.CreateConnection();
        var rows = (await conn.QueryAsync<PriorityRow>(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken))).ToList();

        return rows.Select(row =>
        {
            var priority = Enum.IsDefined(typeof(PriorityEnums), row.Priority)
                ? (PriorityEnums)row.Priority
                : PriorityEnums.Low;

            return new PriorityCountDto
            {
                Priority = priority,
                Count = (int)row.Count
            };
        }).ToList();
    }

    // ─── Category Counts ──────────────────────────────────────────────────────
    public async Task<IReadOnlyList<CategoryCountDto>> GetCategoryCountsAsync(
        DashboardQueryParams queryParams,
        CancellationToken cancellationToken = default)
    {
        if (queryParams is null) throw new ArgumentNullException(nameof(queryParams));

        var (where, parameters) = BuildWhereClause(queryParams);

        var sql = $@"
            SELECT rc.category_name AS CategoryName, COUNT(1) AS Count
            FROM service_requests sr
            INNER JOIN request_categories rc ON sr.category_id = rc.category_id
            {where}
            GROUP BY rc.category_name
            HAVING COUNT(1) > 0
            ORDER BY COUNT(1) DESC;";

        using var conn = _dapperContext.CreateConnection();
        var rows = (await conn.QueryAsync<CategoryRow>(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken))).ToList();

        return rows.Select(row => new CategoryCountDto
        {
            CategoryName = row.CategoryName ?? string.Empty,
            Count = (int)row.Count
        }).ToList();
    }

    // ─── Assigned User Counts ─────────────────────────────────────────────────
    public async Task<IReadOnlyList<AssignedUserCountDto>> GetAssignedUserCountsAsync(
        DashboardQueryParams queryParams,
        CancellationToken cancellationToken = default)
    {
        if (queryParams is null) throw new ArgumentNullException(nameof(queryParams));

        var (where, parameters) = BuildWhereClause(queryParams);

        var extendedWhere = string.IsNullOrEmpty(where)
            ? "WHERE sr.assigned_to IS NOT NULL"
            : where + " AND sr.assigned_to IS NOT NULL";

        var sql = $@"
            SELECT
                u.user_id   AS UserId,
                u.full_name AS FullName,
                COUNT(1)    AS Count
            FROM service_requests sr
            INNER JOIN users u ON sr.assigned_to = u.user_id
            {extendedWhere}
            GROUP BY u.user_id, u.full_name
            HAVING COUNT(1) > 0
            ORDER BY COUNT(1) DESC;";

        using var conn = _dapperContext.CreateConnection();
        var rows = (await conn.QueryAsync<AssignedUserRow>(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken))).ToList();

        return rows.Select(row => new AssignedUserCountDto
        {
            UserId = row.UserId,
            FullName = row.FullName ?? string.Empty,
            Count = (int)row.Count
        }).ToList();
    }

    // ─── WHERE Clause Builder ─────────────────────────────────────────────────
    private static (string whereClause, DynamicParameters parameters) BuildWhereClause(
        DashboardQueryParams queryParams)
    {
        var where = new List<string>();
        var parameters = new DynamicParameters();

        if (queryParams.FromDate.HasValue)
        {
            where.Add("sr.created_on >= @FromDate");
            parameters.Add("@FromDate", queryParams.FromDate.Value.ToUniversalTime());
        }

        if (queryParams.ToDate.HasValue)
        {
            where.Add("sr.created_on <= @ToDate");
            parameters.Add("@ToDate", queryParams.ToDate.Value.ToUniversalTime());
        }

        if (queryParams.EmployeeId.HasValue)
        {
            where.Add("sr.employee_id = @EmployeeId");
            parameters.Add("@EmployeeId", queryParams.EmployeeId.Value);
        }
        else if (queryParams.AssignedToUserId.HasValue)
        {
            where.Add("sr.assigned_to = @AssignedToUserId");
            parameters.Add("@AssignedToUserId", queryParams.AssignedToUserId.Value);
        }
        else if (queryParams.ManagerId.HasValue)
        {
            where.Add("EXISTS (SELECT 1 FROM users u WHERE u.user_id = sr.employee_id AND u.manager_id = @ManagerId)");
            parameters.Add("@ManagerId", queryParams.ManagerId.Value);
        }

        var whereClause = where.Count > 0
            ? "WHERE " + string.Join(" AND ", where)
            : string.Empty;

        return (whereClause, parameters);
    }
}