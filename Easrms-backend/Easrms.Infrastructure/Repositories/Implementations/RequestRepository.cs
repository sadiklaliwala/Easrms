using Dapper;
using Easrms.Application.DTOs.Common;
using Easrms.Application.DTOs.Request;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Common.Enums;
using Easrms.Domain.Entities;
using Easrms.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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

    public RequestRepository(AppDbContext dbContext, DapperContext dapperContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _dapperContext = dapperContext ?? throw new ArgumentNullException(nameof(dapperContext));
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
            whereClauses.Add("(sr.RequestNumber LIKE @RequestNumberPrefix OR sr.Title LIKE @TitleSearch)");
            parameters.Add("@RequestNumberPrefix", queryParams.SearchTerm + "%");
            parameters.Add("@TitleSearch", "%" + queryParams.SearchTerm + "%");
        }

        // Status filter: query param is the int value of the enum
        if (!string.IsNullOrWhiteSpace(queryParams.Status))
        {
            if (int.TryParse(queryParams.Status, out var statusInt))
            {
                whereClauses.Add("sr.Status = @Status");
                parameters.Add("@Status", statusInt);
            }
            else if (Enum.TryParse<RequestStatusEnum>(queryParams.Status, true, out var statusEnum))
            {
                whereClauses.Add("sr.Status = @Status");
                parameters.Add("@Status", (int)statusEnum);
            }
        }

        // Priority filter: query param is the int value of the enum
        if (!string.IsNullOrWhiteSpace(queryParams.Priority))
        {
            if (int.TryParse(queryParams.Priority, out var priorityInt))
            {
                whereClauses.Add("sr.Priority = @Priority");
                parameters.Add("@Priority", priorityInt);
            }
            else if (Enum.TryParse<PriorityEnums>(queryParams.Priority, true, out var priorityEnum))
            {
                whereClauses.Add("sr.Priority = @Priority");
                parameters.Add("@Priority", (int)priorityEnum);
            }
        }

        if (queryParams.CategoryId.HasValue)
        {
            whereClauses.Add("sr.CategoryId = @CategoryId");
            parameters.Add("@CategoryId", queryParams.CategoryId.Value);
        }

        if (queryParams.EmployeeId.HasValue)
        {
            whereClauses.Add("sr.EmployeeId = @EmployeeId");
            parameters.Add("@EmployeeId", queryParams.EmployeeId.Value);
        }

        if (queryParams.AssignedTo.HasValue)
        {
            whereClauses.Add("sr.AssignedTo = @AssignedTo");
            parameters.Add("@AssignedTo", queryParams.AssignedTo.Value);
        }
        if (queryParams.ManagerId.HasValue)
        {
            whereClauses.Add("EXISTS (SELECT 1 FROM Users u WHERE u.UserId = sr.EmployeeId AND u.ManagerId = @ManagerId)");
            parameters.Add("@ManagerId", queryParams.ManagerId.Value);
        }

        if (queryParams.FromDate.HasValue)
        {
            whereClauses.Add("sr.CreatedOn >= @FromDate");
            parameters.Add("@FromDate", queryParams.FromDate.Value.ToUniversalTime());
        }

        if (queryParams.ToDate.HasValue)
        {
            whereClauses.Add("sr.CreatedOn <= @ToDate");
            parameters.Add("@ToDate", queryParams.ToDate.Value.ToUniversalTime());
        }

        // Sorting — allowlist only
        var allowedSort = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "CreatedOn",     "sr.CreatedOn"     },
            { "Priority",      "sr.Priority"      },
            { "Status",        "sr.Status"        },
            { "RequestNumber", "sr.RequestNumber" }
        };

        var sortColumn = allowedSort.TryGetValue(queryParams.SortBy ?? string.Empty, out var col)
            ? col
            : "sr.CreatedOn";

        var sortDir = queryParams.SortAscending ? "ASC" : "DESC";

        parameters.Add("@Offset", offset);
        parameters.Add("@PageSize", pageSize);

        var where = string.Join(" AND ", whereClauses);

        var sql = $@"
            SELECT COUNT(1)
            FROM ServiceRequests sr
            WHERE {where};

            SELECT
                sr.RequestId,
                sr.RequestNumber,
                sr.Title,
                rc.CategoryName,
                sr.Priority,
                sr.Status,
                sr.CreatedOn,
                au.FullName AS AssigneeName
            FROM ServiceRequests sr
            LEFT JOIN RequestCategories rc ON sr.CategoryId  = rc.CategoryId
            LEFT JOIN Users            au ON sr.AssignedTo   = au.UserId
            WHERE {where}
            ORDER BY {sortColumn} {sortDir}
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

        using var conn = _dapperContext.CreateConnection();
        using var multi = await conn.QueryMultipleAsync(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));

        var total = await multi.ReadFirstAsync<int>();
        var rows = (await multi.ReadAsync()).ToList();

        var items = rows.Select(row =>
        {
            // DB columns are int — cast directly, no string parsing needed
            Guid requestId = row.RequestId;
            string requestNumber = row.RequestNumber ?? string.Empty;
            string title = row.Title ?? string.Empty;
            string categoryName = row.CategoryName ?? string.Empty;
            int priorityInt = (int)row.Priority;
            int statusInt = (int)row.Status;
            DateTime createdOn = row.CreatedOn;
            string assigneeName = row.AssigneeName ?? string.Empty;

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
                AssigneeName = assigneeName
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
            .FirstOrDefaultAsync(sr => sr.RequestId == requestId, cancellationToken);
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
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}