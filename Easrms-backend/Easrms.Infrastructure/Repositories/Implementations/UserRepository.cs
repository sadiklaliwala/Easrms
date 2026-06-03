using AutoMapper;
using Dapper;
using Easrms.Application.DTOs.User;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Domain.Entities;
using Easrms.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Easrms.Infrastructure.Repositories.Implementations;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _dbContext;
    private readonly DapperContext _dapperContext;
    private readonly IMapper _mapper;

    public UserRepository(
        AppDbContext dbContext,
        DapperContext dapperContext,
        IMapper mapper)
    {
        _dbContext = dbContext;
        _dapperContext = dapperContext;
        _mapper = mapper;
    }

    // ---------------------------------------------------------------------
    // Dapper - Paginated, filtered, searchable, sortable list
    // ---------------------------------------------------------------------
    public async Task<UserListWithPaginationDto> GetAllAsync(
        int pageNumber,
        int pageSize,
        string? search = null,
        Guid? roleId = null,
        bool? isActive = null,
        string? sortBy = null,
        string? sortDirection = null,
        CancellationToken cancellationToken = default)
    {
        // Validate pagination
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);

        // Build dynamic SQL with safe parameterization
        var whereClauses = new List<string> { "u.is_deleted = FALSE" };
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(search))
        {
            whereClauses.Add("(u.full_name LIKE @Search OR u.email LIKE @Search)");
            parameters.Add("@Search", $"%{search}%");
        }

        if (roleId.HasValue)
        {
            whereClauses.Add("u.role_id = @RoleId");
            parameters.Add("@RoleId", roleId.Value);
        }

        if (isActive.HasValue)
        {
            whereClauses.Add("u.is_active = @IsActive");
            parameters.Add("@IsActive", isActive.Value);
        }

        var orderBy = "u.created_on DESC"; // default
        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            // Allowlist sortable columns to prevent SQL injection
            var allowedSortColumns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "FullName", "u.full_name" },
                { "Email", "u.email" },
                { "CreatedOn", "u.created_on" },
                { "IsActive", "u.is_active" }
            };

            if (allowedSortColumns.TryGetValue(sortBy, out var column))
            {
                // Determine sort direction
                var dir = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase) ? "ASC" : "DESC";
                orderBy = $"{column} {dir}";
            }
        }

        var offset = (pageNumber - 1) * pageSize;
        parameters.Add("@Offset", offset);
        parameters.Add("@PageSize", pageSize);

        var where = string.Join(" AND ", whereClauses);

        var sql = $@"
                    SELECT COUNT(1) FROM users u
                    WHERE {where};

                    SELECT u.user_id AS UserId, u.full_name AS FullName, u.email AS Email, r.role_name AS RoleName, u.is_active AS IsActive, u.created_on AS CreatedOn
                    FROM users u
                    LEFT JOIN roles r ON u.role_id = r.role_id
                    WHERE {where}
                    ORDER BY {orderBy}
                    LIMIT @PageSize OFFSET @Offset;
                    ";

        using var conn = _dapperContext.CreateConnection();
        // Execute both queries in a single round-trip
        using var multi = await conn.QueryMultipleAsync(new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));

        var total = await multi.ReadFirstAsync<int>();
        var items = (await multi.ReadAsync<UserListDto>()).ToList();

        var result = new UserListWithPaginationDto
        {
            Items = items,
            Pagination = new Easrms.Application.DTOs.Common.PaginationDto
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize)
            }
        };

        return result;
    }

    // ---------------------------------------------------------------------
    // Dapper - Simple lookups
    // ---------------------------------------------------------------------
    public async Task<IEnumerable<User>> GetSupportUsersAsync(CancellationToken cancellationToken = default)
    {
        var sql = @"SELECT u.* FROM users u
                    WHERE u.is_active = TRUE AND u.is_deleted = FALSE AND u.role_id IN (
                        SELECT r.role_id FROM roles r WHERE r.role_name = @SupportRoleName
                    );";

        using var conn = _dapperContext.CreateConnection();
        var users = await conn.QueryAsync<User>(new CommandDefinition(sql, new { SupportRoleName = "Support" }, cancellationToken: cancellationToken));
        return users;
    }

    public async Task<IEnumerable<User>> GetManagersAsync(CancellationToken cancellationToken = default)
    {
        var sql = @"SELECT u.* FROM users u
WHERE u.is_active = TRUE AND u.is_deleted = FALSE AND u.role_id IN (
    SELECT r.role_id FROM roles r WHERE r.role_name = @ManagerRoleName
);";

        using var conn = _dapperContext.CreateConnection();
        var users = await conn.QueryAsync<User>(new CommandDefinition(sql, new { ManagerRoleName = "Manager" }, cancellationToken: cancellationToken));
        return users;
    }

    // ---------------------------------------------------------------------
    // EF Core - Single entity reads
    // ---------------------------------------------------------------------
    public async Task<User?> GetByIdAsync(Guid userId, bool trackChanges = false, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Users
            .Include(u => u.Role)
            .Include(u => u.Manager)
            .Where(u => u.UserId == userId && !u.IsDeleted);

        if (!trackChanges)
        {
            query = query.AsNoTrackingWithIdentityResolution();
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, bool trackChanges = false, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Users
            .Include(u => u.Role)
            .Where(u => u.Email == email && !u.IsDeleted);

        if (!trackChanges)
        {
            query = query.AsNoTrackingWithIdentityResolution();
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken, bool trackChanges = false, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Users
            .Include(u => u.Role)
            .Where(u => u.RefreshToken == refreshToken && !u.IsDeleted);

        if (!trackChanges)
        {
            query = query.AsNoTrackingWithIdentityResolution();
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    // NEW: GetByEmailForOtpAsync
    public async Task<User?> GetByEmailForOtpAsync(string email)
    {
        return await _dbContext.Users
            .Where(u => u.Email == email && u.IsActive && !u.IsDeleted)
            .FirstOrDefaultAsync();
    }

    // NEW: UpdateUserOtpAsync
    public async Task UpdateUserOtpAsync(Guid userId, string? otpCode, DateTime? otpExpiry)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (user == null)
            return;

        user.OtpCode = otpCode;
        user.OtpExpiryOn = otpExpiry?.ToUniversalTime();
        await _dbContext.SaveChangesAsync();
    }

    // ---------------------------------------------------------------------
    // Dapper - Scalar checks
    // ---------------------------------------------------------------------
    public async Task<bool> EmailExistsAsync(string email, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
    {
        var sql = @"SELECT CASE WHEN EXISTS (
    SELECT 1 FROM users u WHERE u.email = @Email AND (@ExcludeUserId IS NULL OR u.user_id <> @ExcludeUserId) AND u.is_deleted = FALSE
) THEN TRUE ELSE FALSE END";

        using var conn = _dapperContext.CreateConnection();
        var exists = await conn.ExecuteScalarAsync<bool>(new CommandDefinition(sql, new { Email = email, ExcludeUserId = excludeUserId }, cancellationToken: cancellationToken));
        return exists;
    }

    public async Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var sql = "SELECT CASE WHEN EXISTS (SELECT 1 FROM users WHERE user_id = @UserId AND is_deleted = FALSE) THEN TRUE ELSE FALSE END";
        using var conn = _dapperContext.CreateConnection();
        var exists = await conn.ExecuteScalarAsync<bool>(new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));
        return exists;
    }

    // ---------------------------------------------------------------------
    // EF Core - Writes
    // ---------------------------------------------------------------------
    public async Task CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        // Ensure CreatedOn is UTC
        user.CreatedOn = DateTime.UtcNow;

        await _dbContext.Users.AddAsync(user, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        // Update UpdatedOn timestamp
        user.UpdatedOn = DateTime.UtcNow;

        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ToggleStatusAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Use ExecuteUpdateAsync to toggle IsActive column without fetching entity
        await _dbContext.Users
            .Where(u => u.UserId == userId && !u.IsDeleted)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.IsActive, u => !u.IsActive), cancellationToken);
    }
    public async Task UpdateLoginMetaAsync(Guid userId, string refreshToken, DateTime expiryOn, CancellationToken cancellationToken = default)
    {
        await _dbContext.Users
            .Where(u => u.UserId == userId && !u.IsDeleted)
            .ExecuteUpdateAsync(s => s
                .SetProperty(u => u.RefreshToken, u => refreshToken)
                .SetProperty(u => u.RefreshTokenExpiryOn, u => expiryOn.ToUniversalTime())
                .SetProperty(u => u.LastLoginOn, u => DateTime.UtcNow),
            cancellationToken);
    }

    public async Task UpdateLastLoginAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await _dbContext.Users
            .Where(u => u.UserId == userId && !u.IsDeleted)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.LastLoginOn, u => DateTime.UtcNow), cancellationToken);
    }

    public async Task UpdateRefreshTokenAsync(Guid userId, string refreshToken, DateTime expiryOn, CancellationToken cancellationToken = default)
    {
        // Caller must provide already hashed refreshToken
        await _dbContext.Users
            .Where(u => u.UserId == userId && !u.IsDeleted)
            .ExecuteUpdateAsync(s => s
                .SetProperty(u => u.RefreshToken, u => refreshToken)
                .SetProperty(u => u.RefreshTokenExpiryOn, u => expiryOn.ToUniversalTime()), cancellationToken);
    }

    public async Task RevokeRefreshTokenAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await _dbContext.Users
            .Where(u => u.UserId == userId && !u.IsDeleted)
            .ExecuteUpdateAsync(s => s
                .SetProperty(u => u.RefreshToken, u => null)
                .SetProperty(u => u.RefreshTokenExpiryOn, u => null), cancellationToken);
    }

    public async Task<bool> SoftDeleteUserAsync(Guid userId)
    {
        var affected = await _dbContext.Users
            .Where(u => u.UserId == userId && !u.IsDeleted)
            .ExecuteUpdateAsync(s => s
                .SetProperty(u => u.IsDeleted, u => true)
                .SetProperty(u => u.DeletedOn, u => DateTime.UtcNow));

        return affected > 0;
    }
}

