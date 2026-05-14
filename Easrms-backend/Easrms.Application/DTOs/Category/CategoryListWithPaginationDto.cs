// Easrms.Application/DTOs/Category/CategoryListWithPaginationDto.cs

using Easrms.Application.DTOs.Common;

namespace Easrms.Application.DTOs.Category;

/// <summary>
/// Paginated response wrapper returned by <c>ICategoryRepository.GetPagedCategoriesAsync</c>.
/// Mirrors the pattern of <see cref="RequestListWithPaginationDto"/> for consistency across all list endpoints.
/// </summary>
public sealed class CategoryListWithPaginationDto
{
    /// <summary>
    /// The current page of category records.
    /// </summary>
    public IReadOnlyList<CategoryListDto> Items { get; init; } = [];

    /// <summary>
    /// Pagination metadata: PageNumber, PageSize, TotalCount, TotalPages.
    /// </summary>
    public PaginationDto Pagination { get; init; } = new();
}
