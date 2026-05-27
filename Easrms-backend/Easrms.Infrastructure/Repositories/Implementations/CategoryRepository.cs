// Easrms.Infrastructure/Repositories/Implementations/CategoryRepository.cs
using Dapper;
using Easrms.Application.DTOs.Category;
using Easrms.Application.DTOs.Common;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Domain.Entities;
using Easrms.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Easrms.Infrastructure.Repositories.Implementations;

/// <summary>
/// Infrastructure implementation of <see cref="ICategoryRepository"/>.
/// - Uses Dapper for paginated list queries to maximize read performance.
/// - Uses EF Core for single-entity reads and all write operations.
/// - SaveChangesAsync is intentionally left to the caller (handler) to maintain unit-of-work boundaries.
/// </summary>
public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _dbContext;
    private readonly DapperContext _dapperContext;

    public CategoryRepository(AppDbContext dbContext, DapperContext dapperContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _dapperContext = dapperContext ?? throw new ArgumentNullException(nameof(dapperContext));
    }

    /// <summary>
    /// Returns paginated categories using Dapper with parameterized SQL.
    /// Supports search by CategoryName, filtering by IsActive and IsApprovalRequired, sorting, and pagination.
    /// </summary>
    public async Task<CategoryListWithPaginationDto> GetPagedCategoriesAsync(CategoryQueryParams queryParams, CancellationToken cancellationToken = default)
    {
        if (queryParams is null) throw new ArgumentNullException(nameof(queryParams));

        var pageNumber = Math.Max(1, queryParams.PageNumber);
        var pageSize = Math.Clamp(queryParams.PageSize, 1, 100);
        var offset = (pageNumber - 1) * pageSize;

        var whereClauses = new List<string> { "rc.IsDeleted = 0" };
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(queryParams.SearchTerm))
        {
            whereClauses.Add("rc.CategoryName LIKE @SearchTerm");
            parameters.Add("@SearchTerm", "%" + queryParams.SearchTerm + "%");
        }

        if (queryParams.IsActive.HasValue)
        {
            whereClauses.Add("rc.IsActive = @IsActive");
            parameters.Add("@IsActive", queryParams.IsActive.Value);
        }

        if (queryParams.IsApprovalRequired.HasValue)
        {
            whereClauses.Add("rc.IsApprovalRequired = @IsApprovalRequired");
            parameters.Add("@IsApprovalRequired", queryParams.IsApprovalRequired.Value);
        }

        // Sorting allowlist
        var allowedSort = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "CreatedOn", "rc.CreatedOn" },
            { "CategoryName", "rc.CategoryName" }
        };

        var sortColumn = allowedSort.TryGetValue(queryParams.SortBy ?? string.Empty, out var col) ? col : "rc.CreatedOn";
        var sortDir = queryParams.SortAscending ? "ASC" : "DESC";

        parameters.Add("@Offset", offset);
        parameters.Add("@PageSize", pageSize);

        var where = string.Join(" AND ", whereClauses);

        var sql = $@"
SELECT COUNT(1) FROM RequestCategories rc
WHERE {where};

SELECT rc.CategoryId, rc.CategoryName, rc.IsApprovalRequired, rc.IsActive, rc.CreatedOn
FROM RequestCategories rc
WHERE {where}
ORDER BY {sortColumn} {sortDir}
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

        using var conn = _dapperContext.CreateConnection();
        using var multi = await conn.QueryMultipleAsync(new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));

        var total = await multi.ReadFirstAsync<int>();
        var rows = await multi.ReadAsync();

        var items = new List<CategoryListDto>();
        foreach (var row in rows)
        {
            items.Add(new CategoryListDto
            {
                CategoryId = row.CategoryId,
                CategoryName = row.CategoryName ?? string.Empty,
                IsApprovalRequired = row.IsApprovalRequired,
                IsActive = row.IsActive,
                CreatedOn = row.CreatedOn
            });
        }

        var result = new CategoryListWithPaginationDto
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
    /// Get category by id using EF Core. No heavy navigations required for category detail.
    /// </summary>
    public async Task<RequestCategory?> GetByIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.RequestCategories
            .Where(rc => rc.CategoryId == categoryId && !rc.IsDeleted)
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Returns all active categories as lightweight DTOs for dropdowns.
    /// Uses EF Core projection with AsNoTracking for minimal overhead.
    /// </summary>
    public async Task<IReadOnlyList<CategoryListDto>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        var items = await _dbContext.RequestCategories
            .Where(rc => rc.IsActive && !rc.IsDeleted)
            .AsNoTracking()
            .Select(rc => new CategoryListDto
            {
                CategoryId = rc.CategoryId,
                CategoryName = rc.CategoryName,
                IsApprovalRequired = rc.IsApprovalRequired,
                IsActive = rc.IsActive,
                CreatedOn = rc.CreatedOn
            })
            .ToListAsync(cancellationToken);

        return items;
    }

    public async Task<bool> ExistsAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.RequestCategories.AnyAsync(rc => rc.CategoryId == categoryId && !rc.IsDeleted, cancellationToken);
    }

    public async Task<bool> IsCategoryNameTakenAsync(string categoryName, Guid? excludeCategoryId = null, CancellationToken cancellationToken = default)
    {
        var normalized = categoryName.Trim().Replace(" ", "").ToLowerInvariant();

        return await _dbContext.RequestCategories
            .AnyAsync(rc => rc.CategoryName.Replace(" ", "").ToLower() == normalized
                         && (excludeCategoryId == null || rc.CategoryId != excludeCategoryId.Value)
                         && !rc.IsDeleted, cancellationToken);
    }

    /// <summary>
    /// Adds a new RequestCategory to the DbContext. Does not call SaveChangesAsync — caller must save.
    /// </summary>
    public async Task AddAsync(RequestCategory category, CancellationToken cancellationToken = default)
    {
        if (category is null) throw new ArgumentNullException(nameof(category));

        category.CreatedOn = DateTime.UtcNow;
        await _dbContext.RequestCategories.AddAsync(category, cancellationToken);
    }

    /// <summary>
    /// Marks RequestCategory as modified in the EF Core change tracker.
    /// </summary>
    public void Update(RequestCategory category)
    {
        if (category is null) throw new ArgumentNullException(nameof(category));

        category.UpdatedOn = DateTime.UtcNow;
        _dbContext.RequestCategories.Update(category);
    }

    /// <summary>
    /// Persists pending EF Core changes. The caller (handler) controls the transaction boundary.
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> SoftDeleteCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var affected = await _dbContext.RequestCategories
            .Where(rc => rc.CategoryId == categoryId && !rc.IsDeleted)
            .ExecuteUpdateAsync(s => s
                .SetProperty(rc => rc.IsDeleted, rc => true)
                .SetProperty(rc => rc.DeletedOn, rc => DateTime.UtcNow), cancellationToken);

        return affected > 0;
    }
}

