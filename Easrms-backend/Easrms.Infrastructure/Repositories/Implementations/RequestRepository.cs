using Dapper;
using Easrms.Application.DTOs.Common;
using Easrms.Application.DTOs.Request;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Common.Enums;
using Easrms.Domain.Entities;
using Easrms.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Easrms.Infrastructure.Elastic;
using Easrms.Infrastructure.Elastic.ElasticDocuments;

namespace Easrms.Infrastructure.Repositories.Implementations;

/// <summary>
/// Repository implementation for ServiceRequest operations.
/// - Uses Dapper for complex paginated read queries.
/// - Uses EF Core for single-entity reads and all write operations.
/// - Handlers own the unit-of-work boundary: SaveChangesAsync is called by the handler.
/// </summary>
public class RequestRepository : IRequestRepository
{
    private readonly AppDbContext _dbContext;
    private readonly DapperContext _dapperContext;
    private readonly IElasticService _elasticService;

    public RequestRepository(AppDbContext dbContext, DapperContext dapperContext, IElasticService elasticService)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _dapperContext = dapperContext ?? throw new ArgumentNullException(nameof(dapperContext));
        _elasticService = elasticService;
    }

    /// <summary>
    /// Get paged service requests using Dapper.
    /// Supports filtering, searching, sorting and pagination.
    /// Priority and Status are stored as int (enum values) in DB.
    /// </summary>
    public async Task<RequestListWithPaginationDto> GetPagedRequestsAsync(
        RequestQueryParams queryParams,
        CancellationToken cancellationToken = default)
    {
        if (queryParams is null) throw new ArgumentNullException(nameof(queryParams));

        // Validate paging inputs
        var pageNumber = Math.Max(1, queryParams.PageNumber);
        var pageSize = Math.Clamp(queryParams.PageSize, 1, 10);
        var offset = (pageNumber - 1) * pageSize;

        // Build WHERE clauses and parameters
        var whereClauses = new List<string> { "1=1" };
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(queryParams.SearchTerm))
        {
            whereClauses.Add("(sr.request_number LIKE @RequestNumberPrefix OR sr.title LIKE @TitleSearch)");
            parameters.Add("@RequestNumberPrefix", queryParams.SearchTerm + "%");
            parameters.Add("@TitleSearch", "%" + queryParams.SearchTerm + "%");
        }

        // Status filter: query param is the int value of the enum
        if (!string.IsNullOrWhiteSpace(queryParams.Status))
        {
            if (int.TryParse(queryParams.Status, out var statusInt))
            {
                whereClauses.Add("sr.status = @Status");
                parameters.Add("@Status", statusInt);
            }
            else if (Enum.TryParse<RequestStatusEnum>(queryParams.Status, true, out var statusEnum))
            {
                whereClauses.Add("sr.status = @Status");
                parameters.Add("@Status", (int)statusEnum);
            }
        }

        // Priority filter: query param is the int value of the enum
        if (!string.IsNullOrWhiteSpace(queryParams.Priority))
        {
            if (int.TryParse(queryParams.Priority, out var priorityInt))
            {
                whereClauses.Add("sr.priority = @Priority");
                parameters.Add("@Priority", priorityInt);
            }
            else if (Enum.TryParse<PriorityEnums>(queryParams.Priority, true, out var priorityEnum))
            {
                whereClauses.Add("sr.priority = @Priority");
                parameters.Add("@Priority", (int)priorityEnum);
            }
        }

        if (queryParams.CategoryId.HasValue)
        {
            whereClauses.Add("sr.category_id = @CategoryId");
            parameters.Add("@CategoryId", queryParams.CategoryId.Value);
        }

        if (queryParams.EmployeeId.HasValue)
        {
            whereClauses.Add("sr.employee_id = @EmployeeId");
            parameters.Add("@EmployeeId", queryParams.EmployeeId.Value);
        }

        if (queryParams.AssignedTo.HasValue)
        {
            whereClauses.Add("sr.assigned_to = @AssignedTo");
            parameters.Add("@AssignedTo", queryParams.AssignedTo.Value);
        }
        if (queryParams.ManagerId.HasValue)
        {
            whereClauses.Add("EXISTS (SELECT 1 FROM users u WHERE u.user_id = sr.employee_id AND u.manager_id = @ManagerId)");
            parameters.Add("@ManagerId", queryParams.ManagerId.Value);
        }

        if (queryParams.FromDate.HasValue)
        {
            whereClauses.Add("sr.created_on >= @FromDate");
            parameters.Add("@FromDate", queryParams.FromDate.Value.ToUniversalTime());
        }

        if (queryParams.ToDate.HasValue)
        {
            whereClauses.Add("sr.created_on <= @ToDate");
            parameters.Add("@ToDate", queryParams.ToDate.Value.ToUniversalTime());
        }

        // Sorting — allowlist only
        var allowedSort = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "RequestNumber", "sr.request_number" },
            { "Title",         "sr.title" },
            { "Status",        "sr.status" },
            { "Priority",      "sr.priority" },
            { "CreatedOn",     "sr.created_on" },
            { "DueDate",       "sr.due_date" },
            { "CategoryName",  "rc.category_name" },
            { "AssigneeName",  "au.full_name" }
        };

        var sortColumn = allowedSort.TryGetValue(queryParams.SortBy ?? string.Empty, out var col)
            ? col
            : "sr.created_on";

        var sortDir = queryParams.SortAscending ? "ASC" : "DESC";

        parameters.Add("@Offset", offset);
        parameters.Add("@PageSize", pageSize);

        var where = string.Join(" AND ", whereClauses);

        var sql = $@"
            SELECT COUNT(1)
            FROM service_requests sr
            WHERE {where};

            SELECT
                sr.request_id AS RequestId,
                sr.request_number AS RequestNumber,
                sr.title AS Title,
                rc.category_name AS CategoryName,
                sr.priority AS Priority,
                sr.status AS Status,
                sr.created_on AS CreatedOn,
                emp.full_name AS EmployeeName,
                au.full_name AS AssigneeName,
                sr.due_date AS DueDate,
                sr.is_escalated AS IsEscalated,
                sr.escalated_on AS EscalatedOn,
                sr.escalation_reason AS EscalationReason,
                u_esc.full_name AS EscalatedByName,
                CASE
                  WHEN sr.status IN (7, 8) THEN 'Within SLA'
                  WHEN sr.due_date IS NULL THEN 'N/A'
                  WHEN NOW() > sr.due_date THEN 'Breached'
                  WHEN NOW() > (sr.due_date - INTERVAL '2 hours') THEN 'Nearing Breach'
                  ELSE 'Within SLA'
                END AS SLAStatus,
                sr.attachment_url AS AttachmentUrl
            FROM service_requests sr
            LEFT JOIN request_categories rc ON sr.category_id  = rc.category_id
            LEFT JOIN users            emp ON sr.employee_id = emp.user_id
            LEFT JOIN users            au ON sr.assigned_to   = au.user_id
            LEFT JOIN users            u_esc ON u_esc.user_id = sr.escalated_by
            WHERE {where}
            ORDER BY {sortColumn} {sortDir}
            LIMIT @PageSize OFFSET @Offset;";

        using var conn = _dapperContext.CreateConnection();
        using var multi = await conn.QueryMultipleAsync(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));

        var total = await multi.ReadFirstAsync<int>();
        var rows = (await multi.ReadAsync()).ToList();

        var items = rows.Select(row =>
        {
            // DB columns are int — cast directly, no string parsing needed
            Guid requestId = row.requestid;
            string requestNumber = row.requestnumber ?? string.Empty;
            string title = row.title ?? string.Empty;
            string categoryName = row.categoryname ?? string.Empty;
            int priorityInt = (int)row.priority;
            int statusInt = (int)row.status;
            DateTime createdOn = row.createdon;
            string employeeName = row.employeename ?? string.Empty;
            string assigneeName = row.assignename ?? string.Empty;
            string attachmentUrl = row.attachmenturl ?? string.Empty;

            // New SLA / escalation fields
            DateTime? dueDate = row.duedate is null ? null : (DateTime?)row.duedate;
            bool isEscalated = row.isescalated is not null && (bool)row.isescalated;
            string slaStatus = row.slastatus ?? string.Empty;

            // Safe cast: fall back to sensible defaults if the DB value is somehow out of range
            var priority = Enum.IsDefined(typeof(PriorityEnums), priorityInt)
                ? (PriorityEnums)priorityInt
                : PriorityEnums.Low;

            var status = Enum.IsDefined(typeof(RequestStatusEnum), statusInt)
                ? (RequestStatusEnum)statusInt
                : RequestStatusEnum.Open;

            return new RequestListDto
            {
                RequestId = requestId,
                RequestNumber = requestNumber,
                Title = title,
                CategoryName = categoryName,
                Priority = priority,
                Status = status,
                CreatedOn = createdOn,
                EmployeeName = employeeName,
                AssigneeName = assigneeName,
                DueDate = dueDate,
                SLAStatus = slaStatus,
                IsEscalated = isEscalated,
                AttachmentUrl = attachmentUrl
            };
        }).ToList();

        return new RequestListWithPaginationDto
        {
            Items = items,
            Pagination = new PaginationDto
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize)
            }
        };
    }

    /// <summary>
    /// Get a ServiceRequest by id with all required navigations loaded.
    /// Returns a tracked entity so handlers may modify and call SaveChangesAsync.
    /// EF Core maps int columns to enum properties automatically via value conversion.
    /// </summary>
    public async Task<ServiceRequest?> GetRequestByIdAsync(
        Guid requestId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ServiceRequests
            .Include(sr => sr.Employee)
            .Include(sr => sr.Category)
            .Include(sr => sr.AssignedUser)
            .Include(sr => sr.ClosedByUser)
            .Include(sr => sr.Escalator)
            .FirstOrDefaultAsync(sr => sr.RequestId == requestId, cancellationToken);
    }


    public async Task<ServiceRequest?> GetRequestByNumberAsync(string requestNumber)
    {
        var entity = await _dbContext.ServiceRequests
            .Include(r => r.Employee)
            .Include(r => r.AssignedUser)
            .Include(r => r.Category)
            .FirstOrDefaultAsync(r => r.RequestNumber == requestNumber.ToUpper());

        if (entity is null) return null;

        //return _mapper.Map<RequestDetailDto>(entity);
        return entity;

    }

    /// <summary>
    /// Lightweight fetch with only Category navigation loaded.
    /// Read-only (no tracking) — used for approval checks.
    /// </summary>
    public async Task<ServiceRequest?> GetRequestWithCategoryAsync(
        Guid requestId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ServiceRequests
            .Include(sr => sr.Category)
            .Where(sr => sr.RequestId == requestId)
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        Guid requestId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ServiceRequests
            .AnyAsync(sr => sr.RequestId == requestId, cancellationToken);
    }

    public async Task<bool> IsRequestNumberExistsAsync(
        string requestNumber,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ServiceRequests
            .AnyAsync(sr => sr.RequestNumber == requestNumber, cancellationToken);
    }

    // New: check active requests for category
    public async Task<bool> HasActiveRequestsForCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        // Active = status not Closed (8) and not Rejected (4)
        return await _dbContext.ServiceRequests
            .Where(sr => sr.CategoryId == categoryId && sr.Status != (RequestStatusEnum)8 && sr.Status != (RequestStatusEnum)4)
            .AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Adds request to DbContext. Does NOT call SaveChanges — handler must call SaveChangesAsync.
    /// </summary>
    public async Task AddAsync(ServiceRequest request, CancellationToken cancellationToken = default)
    {
        request.CreatedOn = DateTime.UtcNow;
        await _dbContext.ServiceRequests.AddAsync(request, cancellationToken);
    }

    /// <summary>
    /// Marks request as modified in the change tracker. Synchronous by design.
    /// </summary>
    public void Update(ServiceRequest request)
    {
        request.UpdatedOn = DateTime.UtcNow;
        _dbContext.ServiceRequests.Update(request);
    }

    /// <summary>
    /// Persists pending changes. The handler calls this to complete the unit of work.
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Capture changed ServiceRequest entries before SaveChanges
        var changedEntries = _dbContext.ChangeTracker.Entries<ServiceRequest>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
            .Select(e => new { Id = e.Entity.RequestId, State = e.State })
            .ToList();

        var result = await _dbContext.SaveChangesAsync(cancellationToken);

        if (_elasticService != null && changedEntries.Any())
        {
            foreach (var entry in changedEntries)
            {
                try
                {
                    if (entry.State == EntityState.Deleted)
                    {
                        await _elasticService.DeleteRequestAsync(entry.Id);
                        continue;
                    }

                    // For Added/Modified: fetch fresh entity with navigations
                    var sr = await GetRequestByIdAsync(entry.Id, cancellationToken);
                    if (sr == null) continue;

                    var doc = new RequestDocument
                    {
                        RequestId = sr.RequestId,
                        RequestNumber = sr.RequestNumber ?? string.Empty,
                        Title = sr.Title ?? string.Empty,
                        Description = sr.Description ?? string.Empty,
                        CategoryName = sr.Category?.CategoryName ?? string.Empty,
                        EmployeeName = sr.Employee?.FullName ?? string.Empty,
                        AssigneeName = sr.AssignedUser?.FullName ?? string.Empty,
                        Priority = sr.Priority.ToString(),
                        Status = sr.Status.ToString(),
                        RejectionReason = sr.EscalationReason ?? string.Empty,
                        CreatedOn = sr.CreatedOn,
                        DueDate = sr.DueDate,
                        IsSLABreached = sr.IsEscalated,
                        IsEscalated = sr.IsEscalated
                    };

                    if (entry.State == EntityState.Added)
                        await _elasticService.IndexRequestAsync(doc);
                    else
                        await _elasticService.UpdateRequestAsync(doc);
                }
                catch
                {
                    // Swallow exceptions from ES to avoid breaking application flow.
                    // Consider logging or queuing for retries in production.
                }
            }
        }

        return result;
    }
}