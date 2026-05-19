using AutoMapper;
using Easrms.Application.DTOs.Category;
using Easrms.Application.Interfaces.Repositories;
using MediatR;

namespace Easrms.Application.Features.Category.Queries;

/// <summary>
/// Returns the full detail of a single category.
/// </summary>
public sealed class GetCategoryByIdQuery : IRequest<CategoryDetailDto>
{
    public Guid CategoryId { get; init; }
}


/// <summary>
/// Step-by-step per HANDLER_REPO_REFERENCE_MAP:
///   1. GetByIdAsync(categoryId, trackChanges: false) → 404 if null
///   2. Map entity → CategoryDetailDto → return
/// </summary>
public sealed class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, CategoryDetailDto>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public GetCategoryByIdQueryHandler(ICategoryRepository categoryRepository, IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<CategoryDetailDto> Handle(
        GetCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Fetch — no tracking needed for a read
        var category = await _categoryRepository.GetByIdAsync(
            request.CategoryId,
            cancellationToken: cancellationToken)
            ?? throw new KeyNotFoundException(
                $"Category with id '{request.CategoryId}' was not found.");

        // 2. Map and return
        return _mapper.Map<CategoryDetailDto>(category);
    }
}