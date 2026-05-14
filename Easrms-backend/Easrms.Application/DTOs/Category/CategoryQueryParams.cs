// Easrms.Application/DTOs/Category/CategoryQueryParams.cs

namespace Easrms.Application.DTOs.Category;

/// <summary>
/// Strongly-typed query parameter object passed into <c>ICategoryRepository.GetPagedCategoriesAsync</c>.
/// Encapsulates all filtering, searching, sorting, and pagination inputs in a single immutable model.
/// All filter properties are nullable; null means "no filter applied" for that field.
/// </summary>
public sealed class CategoryQueryParams
{
    // ─────────────────────────────────────────────────────────────────────────
    // PAGINATION
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// The 1-based page number to return. Defaults to 1.
    /// </summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// Number of records per page. Defaults to 10. Maximum enforced at handler level: 100.
    /// </summary>
    public int PageSize { get; init; } = 10;

    // ─────────────────────────────────────────────────────────────────────────
    // SEARCH
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Free-text search against CategoryName (contains, case-insensitive).
    /// Null or empty string disables search.
    /// </summary>
    public string? SearchTerm { get; init; }

    // ─────────────────────────────────────────────────────────────────────────
    // FILTERS
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Filter by active status.
    /// <c>true</c> returns only active categories, <c>false</c> returns only inactive.
    /// Null returns all regardless of active status.
    /// </summary>
    public bool? IsActive { get; init; }

    /// <summary>
    /// Filter by whether approval is required.
    /// <c>true</c> returns only approval-required categories, <c>false</c> returns only non-approval.
    /// Null returns all.
    /// </summary>
    public bool? IsApprovalRequired { get; init; }

    // ─────────────────────────────────────────────────────────────────────────
    // SORT
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Column to sort by. Allowed values: "CreatedOn", "CategoryName".
    /// Defaults to "CreatedOn". Invalid values fall back to default at the Dapper query level.
    /// </summary>
    public string SortBy { get; init; } = "CreatedOn";

    /// <summary>
    /// Sort direction. <c>true</c> = ascending, <c>false</c> = descending.
    /// Defaults to <c>false</c> (newest first).
    /// </summary>
    public bool SortAscending { get; init; } = false;
}
