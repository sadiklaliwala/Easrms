using Easrms.Application.DTOs.User;
using Easrms.Domain.Entities;

namespace Easrms.Application.Interfaces.Repositories;

/// <summary>
/// User repository interface.
///
/// Strategy:
///   EF Core  — writes and identity/navigation reads
///               (change tracking, concurrency tokens, eager loading)
///
///   Dapper   — read-only projections and fast scalar checks
///               (no tracking overhead, raw SQL speed)
///
/// Bulk-write operations (toggle, last-login, refresh-token)
/// use EF Core ExecuteUpdateAsync so they issue a direct
/// UPDATE statement without fetching the entity first.
///
/// IMPORTANT:
/// - All DateTime values must be stored in UTC.
/// - Refresh tokens must NEVER be stored in plain text.
/// - Repository layer returns only Domain Entities for single-entity reads.
/// - Pagination is mandatory for all large list queries.
///
/// Required SQL Indexes:
/// - UX_Users_Email            (UNIQUE)
/// - IX_Users_RoleId
/// - IX_Users_IsActive
/// - IX_Users_ManagerId
/// - IX_Users_RefreshToken
/// - IX_Users_CreatedOn
///
/// Concurrency:
/// - Optimistic concurrency enforced through RowVersion token.
/// - DbUpdateConcurrencyException handled in application layer.
///
/// NOTE:
/// Lookup-oriented methods may later move to ILookupRepository
/// if repository responsibilities grow significantly.
/// </summary>
public interface IUserRepository
{
    // -------------------------------------------------------------------------
    // READ — Dapper
    // Reason:
    //   Read-heavy list queries benefit from raw SQL execution,
    //   lower memory allocation, zero EF tracking overhead,
    //   and better pagination performance.
    // -------------------------------------------------------------------------

    /// <summary>
    /// Fetch paginated users with Role included.
    /// Returns a paginated DTO containing UserListDto items and pagination metadata.
    /// </summary>
    Task<UserListWithPaginationDto> GetAllAsync(
        int pageNumber,
        int pageSize,
        string? search = null,
        Guid? roleId = null,
        bool? isActive = null,
        string? sortBy = null,
        string? sortDirection = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetch all active support users with Role included.
    /// Used by: GET /api/lookup/support-users
    /// </summary>
    /// <remarks>
    /// Dapper — filtered list query with Role JOIN.
    /// No tracking overhead.
    /// </remarks>
    Task<IEnumerable<User>> GetSupportUsersAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetch all active managers with Role included.
    /// Used by: GET /api/lookup/managers
    /// </summary>
    Task<IEnumerable<User>> GetManagersAsync(
        CancellationToken cancellationToken = default);

    // -------------------------------------------------------------------------
    // READ — EF Core
    // -------------------------------------------------------------------------

    Task<User?> GetByIdAsync(
        Guid userId,
        bool trackChanges = false,
        CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(
        string email,
        bool trackChanges = false,
        CancellationToken cancellationToken = default);

    Task<User?> GetByRefreshTokenAsync(
        string refreshToken,
        bool trackChanges = false,
        CancellationToken cancellationToken = default);

    // -------------------------------------------------------------------------
    // SCALAR CHECKS — Dapper
    // -------------------------------------------------------------------------

    Task<bool> EmailExistsAsync(
        string email,
        Guid? excludeUserId = null,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    // -------------------------------------------------------------------------
    // WRITE — EF Core (tracked entity operations)
    // -------------------------------------------------------------------------

    Task CreateAsync(
        User user,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        User user,
        CancellationToken cancellationToken = default);

    // -------------------------------------------------------------------------
    // WRITE — EF Core ExecuteUpdateAsync (direct UPDATE statements)
    // -------------------------------------------------------------------------

    Task ToggleStatusAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task UpdateLastLoginAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task UpdateLoginMetaAsync(
    Guid userId,
    string refreshToken,
    DateTime expiryOn,
    CancellationToken cancellationToken = default);

    Task UpdateRefreshTokenAsync(
        Guid userId,
        string refreshToken,
        DateTime expiryOn,
        CancellationToken cancellationToken = default);

    Task RevokeRefreshTokenAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task UpdateUserOtpAsync(Guid userId, string? otpCode, DateTime? otpExpiry);
    Task<User?> GetByEmailForOtpAsync(string email);

    // Soft delete
    Task<bool> SoftDeleteUserAsync(Guid userId);
}