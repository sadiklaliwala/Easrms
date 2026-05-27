using Easrms.Application.DTOs.Category;
using Easrms.Application.Interfaces.Repositories;
using MediatR;

namespace Easrms.Application.Features.Category.Queries;

/// <summary>
/// Returns a paginated list of categories.
/// Supports optional filtering by name and active status.
/// </summary>
public sealed class GetAllCategoriesQuery : IRequest<CategoryListWithPaginationDto>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Search { get; init; }
    public bool? IsActive { get; init; }

    public bool? IsApprovalRequired { get; set; }

    // Sorting
    public string? SortBy { get; init; }
    public bool? SortAscending { get; init; }
    public string? SortDirection { get; init; }
}


/// <summary>
/// Step-by-step per HANDLER_REPO_REFERENCE_MAP:
///   1. GetPagedCategoriesAsync(queryParams)
///      → returns CategoryListWithPaginationDto directly (Dapper projection)
/// </summary>
public sealed class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, CategoryListWithPaginationDto>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetAllCategoriesQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<CategoryListWithPaginationDto> Handle(
        GetAllCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        // Determine SortAscending: prefer explicit SortAscending flag, otherwise derive from SortDirection
        bool sortAsc = request.SortAscending ?? (string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase));

        var queryparams = new CategoryQueryParams()
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SearchTerm = request.Search,
            IsActive = request.IsActive,
            SortBy = request.SortBy ?? "CreatedOn",
            SortAscending = sortAsc,
            IsApprovalRequired = request.IsApprovalRequired,
        };

        // 1. Delegate directly — Dapper handles projection + pagination
        return await _categoryRepository.GetPagedCategoriesAsync(
            queryparams,            
            cancellationToken: cancellationToken);
    }
}