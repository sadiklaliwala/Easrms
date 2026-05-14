// Easrms.Infrastructure/Repositories/Interfaces/ICategoryRepository.cs

using Easrms.Application.DTOs.Category;
using Easrms.Domain.Entities;

namespace Easrms.Application.Interfaces.Repositories;

/// <summary>
/// Repository contract for all RequestCategory data operations.
/// Read operations involving filtering or pagination use Dapper.
/// Write operations and single-record fetches use EF Core.
/// All async methods accept a <see cref="CancellationToken"/> for request cancellation support.
/// </summary>
public interface ICategoryRepository
{
    // ─────────────────────────────────────────────────────────────────────────
    // QUERY METHODS
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns a paginated, filtered list of categories using Dapper for performance.
    /// Supports search by CategoryName (contains), filter by IsActive and IsApprovalRequired.
    /// Sorted by CreatedOn DESC by default.
    /// </summary>
    /// <param name="queryParams">
    /// Strongly-typed object encapsulating all filter, search, sort, and pagination inputs.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>
    /// A <see cref="CategoryListWithPaginationDto"/> containing the current page of
    /// <see cref="CategoryListDto"/> items and full pagination metadata.
    /// </returns>
    Task<CategoryListWithPaginationDto> GetPagedCategoriesAsync(
        CategoryQueryParams queryParams,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the full detail of a single category by its ID using EF Core.
    /// No navigation properties are loaded — category has no heavy navigations needed for detail view.
    /// Returns null if the category does not exist.
    /// </summary>
    /// <param name="categoryId">The unique identifier of the category.</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>
    /// The <see cref="RequestCategory"/> entity, or null if not found.
    /// </returns>
    Task<RequestCategory?> GetByIdAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all active categories as a lightweight list.
    /// Used to populate the category dropdown on the Create Request screen.
    /// Uses AsNoTracking for read-only performance. No pagination — dropdown needs full list.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>
    /// A list of <see cref="CategoryListDto"/> where IsActive is true.
    /// Returns an empty list if none exist.
    /// </returns>
    Task<IReadOnlyList<CategoryListDto>> GetAllActiveAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a category with the given ID exists in the database.
    /// Lightweight — executes a single SQL EXISTS check via EF Core.
    /// Used as a guard in all command handlers before performing any mutation.
    /// </summary>
    /// <param name="categoryId">The unique identifier of the category.</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns><c>true</c> if the category exists; otherwise <c>false</c>.</returns>
    Task<bool> ExistsAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a category with the given name already exists, optionally excluding
    /// a specific category ID from the check.
    /// Used on both Create (excludeId = null) and Edit (excludeId = current category ID)
    /// to prevent duplicate category names while allowing a category to keep its own name.
    /// Case-insensitive comparison applied at the query level.
    /// </summary>
    /// <param name="categoryName">The category name to check for uniqueness.</param>
    /// <param name="excludeCategoryId">
    /// The ID of the category to exclude from the check (used on edit). Pass null on create.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns><c>true</c> if the name is already taken; otherwise <c>false</c>.</returns>
    Task<bool> IsCategoryNameTakenAsync(
        string categoryName,
        Guid? excludeCategoryId = null,
        CancellationToken cancellationToken = default);

    // ─────────────────────────────────────────────────────────────────────────
    // COMMAND METHODS
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Inserts a new <see cref="RequestCategory"/> record into the database.
    /// The entity must have CategoryName, IsApprovalRequired, IsActive, and CreatedOn
    /// already set by the command handler before calling this method.
    /// Does not call SaveChanges — SaveChangesAsync is called by the handler.
    /// </summary>
    /// <param name="category">The fully constructed <see cref="RequestCategory"/> entity to insert.</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    Task AddAsync(
        RequestCategory category,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a <see cref="RequestCategory"/> entity as modified in the EF Core change tracker.
    /// Used by both UpdateCategory and ToggleCategoryStatus command handlers.
    /// The handler fetches the entity, applies property changes, then calls this method
    /// followed by SaveChangesAsync. No additional DB round-trip performed here.
    /// </summary>
    /// <param name="category">The modified <see cref="RequestCategory"/> entity.</param>
    void Update(RequestCategory category);

    /// <summary>
    /// Persists all pending EF Core change tracker entries to the database.
    /// Called once at the end of each command handler to commit the unit of work.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>The number of rows affected.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}