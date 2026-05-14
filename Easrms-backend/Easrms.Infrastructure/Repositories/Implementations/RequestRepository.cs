using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Easrms.Application.DTOs.Common;
using Easrms.Application.DTOs.Request;
using Easrms.Application.Interfaces.Repositories;
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
    /// Get paged service requests using Dapper. Supports filtering, searching, sorting and pagination.
    /// </summary>
    public async Task<RequestListWithPaginationDto> GetPagedRequestsAsync(RequestQueryParams queryParams, CancellationToken cancellationToken = default)
    {
        if (queryParams is null) throw new ArgumentNullException(nameof(queryParams));

        // Validate paging inputs
        var pageNumber = Math.Max(1, queryParams.PageNumber);
        var pageSize = Math.Clamp(queryParams.PageSize, 1, 100);
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

        if (!string.IsNullOrWhiteSpace(queryParams.Status))
        {
            whereClauses.Add("sr.Status = @Status");
            parameters.Add("@Status", queryParams.Status);
        }

        if (!string.IsNullOrWhiteSpace(queryParams.Priority))
        {
            whereClauses.Add("sr.Priority = @Priority");
            parameters.Add("@Priority", queryParams.Priority);
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

        // Sorting - allowlist columns
        var allowedSort = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "CreatedOn", "sr.CreatedOn" },
            { "Priority", "sr.Priority" },
            { "Status", "sr.Status" },
            { "RequestNumber", "sr.RequestNumber" }
        };

        var sortColumn = allowedSort.ContainsKey(queryParams.SortBy) ? allowedSort[queryParams.SortBy] : "sr.CreatedOn";
        var sortDir = queryParams.SortAscending ? "ASC" : "DESC";

        parameters.Add("@Offset", offset);
        parameters.Add("@PageSize", pageSize);

        var where = string.Join(" AND ", whereClauses);

        var sql = $@"
                    SELECT COUNT(1) FROM ServiceRequests sr
                    WHERE {where};

                    SELECT sr.RequestId, sr.RequestNumber, sr.Title, rc.CategoryName, sr.Priority, sr.Status, sr.CreatedOn, au.FullName as AssigneeName
                    FROM ServiceRequests sr
                    LEFT JOIN RequestCategories rc ON sr.CategoryId = rc.CategoryId
                    LEFT JOIN Users au ON sr.AssignedTo = au.UserId
                    WHERE {where}
                    ORDER BY {sortColumn} {sortDir}
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

        using var conn = _dapperContext.CreateConnection();
        using var multi = await conn.QueryMultipleAsync(new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));

        var total = await multi.ReadFirstAsync<int>();
        var rows = await multi.ReadAsync();

        var items = new List<RequestListDto>();
        foreach (var row in rows)
        {
            // Dapper returns dynamic; safely map fields
            Guid requestId = row.RequestId;
            string requestNumber = row.RequestNumber ?? string.Empty;
            string title = row.Title ?? string.Empty;
            string categoryName = row.CategoryName ?? string.Empty;
            string priorityStr = row.Priority ?? string.Empty;
            string statusStr = row.Status ?? string.Empty;
            DateTime createdOn = row.CreatedOn;
            string assigneeName = row.AssigneeName ?? string.Empty;

            int.TryParse(priorityStr, out var priority);
            int.TryParse(statusStr, out var status);

            items.Add(new RequestListDto
            {
                RequestId = requestId,
                RequestNumber = requestNumber,
                Title = title,
                CategoryName = categoryName,
                Priority = priority,
                Status = status,
                CreatedOn = createdOn,
                AssigneeName = assigneeName
            });
        }

        var result = new RequestListWithPaginationDto
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

        return result;
    }

    /// <summary>
    /// Get a ServiceRequest by id with all required navigations loaded for detail views and mutation flows.
    /// Returns tracked entity so handlers may modify it and call SaveChangesAsync.
    /// </summary>
    public async Task<ServiceRequest?> GetRequestByIdAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ServiceRequests
            .Include(sr => sr.Employee)
            .Include(sr => sr.Category)
            .Include(sr => sr.AssignedUser)
            .Include(sr => sr.ClosedByUser)
            .FirstOrDefaultAsync(sr => sr.RequestId == requestId, cancellationToken);
    }

    /// <summary>
    /// Lightweight fetch with only Category navigation loaded. Read-only (no tracking) as it's used for approval checks.
    /// </summary>
    public async Task<ServiceRequest?> GetRequestWithCategoryAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ServiceRequests
            .Include(sr => sr.Category)
            .Where(sr => sr.RequestId == requestId)
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ServiceRequests.AnyAsync(sr => sr.RequestId == requestId, cancellationToken);
    }

    public async Task<bool> IsRequestNumberExistsAsync(string requestNumber, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ServiceRequests.AnyAsync(sr => sr.RequestNumber == requestNumber, cancellationToken);
    }

    /// <summary>
    /// Adds request to the DbContext. Does NOT call SaveChanges — handler should call SaveChangesAsync.
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