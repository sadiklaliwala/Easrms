// Easrms.Infrastructure/Repositories/Interfaces/ILookupRepository.cs


// Easrms.Infrastructure/Repositories/Interfaces/ILookupRepository.cs

using Easrms.Application.DTOs.Lookup;

namespace Easrms.Application.Interfaces.Repositories;

/// <summary>
/// Repository contract for all dropdown lookup data operations.
///
/// This repository is intentionally read-only and has no write methods,
/// no Update(), no SaveChangesAsync(), and no pagination.
/// Lookup data feeds UI dropdowns — it must be flat, fast, and minimal.
///
/// All methods use Dapper for performance. Only active records are returned
/// because inactive users and managers must never appear in dropdowns
/// that drive assignment or user creation flows.
///
/// Caching note:
/// Lookup data is low-volatility (support users and managers change infrequently).
/// The handler layer may wrap these calls with IMemoryCache if needed —
/// the repository itself stays cache-agnostic and always hits the DB.
/// </summary>
public interface ILookupRepository
{
    // ─────────────────────────────────────────────────────────────────────────
    // SUPPORT USERS
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns all active users with the Support User role as a flat list
    /// for the assign-request dropdown on the Admin Assignment screen.
    /// Uses Dapper — joins Users and Roles tables, filters by RoleName = "Support User"
    /// and IsActive = true. Ordered by FullName ASC for consistent dropdown ordering.
    /// Returns only UserId and FullName — no sensitive fields exposed.
    /// Returns empty list if no active support users exist.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>
    /// An ordered list of <see cref="SupportUserLookupDto"/>.
    /// </returns>
    Task<IReadOnlyList<SupportUserLookupDto>> GetActiveSupportUsersAsync(
        CancellationToken cancellationToken = default);

    // ─────────────────────────────────────────────────────────────────────────
    // MANAGERS
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns all active users with the Manager role as a flat list
    /// for the manager dropdown on the Create User and Edit User screens.
    /// Uses Dapper — joins Users and Roles tables, filters by RoleName = "Manager"
    /// and IsActive = true. Ordered by FullName ASC for consistent dropdown ordering.
    /// Returns only UserId and FullName — no sensitive fields exposed.
    /// Returns empty list if no active managers exist.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>
    /// An ordered list of <see cref="ManagerLookupDto"/>.
    /// </returns>
    Task<IReadOnlyList<ManagerLookupDto>> GetActiveManagersAsync(
        CancellationToken cancellationToken = default);

    // ─────────────────────────────────────────────────────────────────────────
    // CATEGORIES (active only — for Create Request dropdown)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns all active request categories as a flat list
    /// for the category dropdown on the Create Request screen.
    /// Uses Dapper — filters RequestCategory by IsActive = true.
    /// Ordered by CategoryName ASC for consistent dropdown ordering.
    ///
    /// Note: This method exists here rather than on ICategoryRepository
    /// because it is consumed by the same LookupController endpoint pattern
    /// and follows the same flat, no-pagination, dropdown contract.
    /// The full paged category list with admin filters lives on ICategoryRepository.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>
    /// An ordered list of <see cref="CategoryLookupDto"/>.
    /// </returns>
    Task<IReadOnlyList<CategoryLookupDto>> GetActiveCategoriesAsync(
        CancellationToken cancellationToken = default);
}