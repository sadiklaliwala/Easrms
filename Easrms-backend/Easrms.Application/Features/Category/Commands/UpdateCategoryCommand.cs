using Easrms.Application.Interfaces.Repositories;
using MediatR;

namespace Easrms.Application.Features.Category.Commands;

/// <summary>
/// Updates name and approval flag of an existing category.
/// Only Admin can dispatch this command (enforced at controller level).
/// </summary>
public sealed class UpdateCategoryCommand : IRequest
{
    public Guid CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public bool IsApprovalRequired { get; init; }
}



/// <summary>
/// Step-by-step per HANDLER_REPO_REFERENCE_MAP:
///   1. GetByIdAsync(categoryId)                                         → 404 if null
///   2. IsCategoryNameTakenAsync(name, excludeCategoryId: categoryId)    → 409 if true
///   3. Apply changes to entity, set UpdatedOn = UtcNow
///   4. Update(entity)
///   5. SaveChangesAsync()
/// </summary>
public sealed class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand>
{
    private readonly ICategoryRepository _categoryRepository;

    public UpdateCategoryCommandHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task Handle(
        UpdateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Fetch existing — 404 if not found
        var category = await _categoryRepository.GetByIdAsync(
            request.CategoryId,
            cancellationToken: cancellationToken)
            ?? throw new KeyNotFoundException(
                $"Category with id '{request.CategoryId}' was not found.");

        // 2. Duplicate name check — exclude self so current name doesn't self-conflict
        var nameTaken = await _categoryRepository.IsCategoryNameTakenAsync(
            request.CategoryName,
            excludeCategoryId: request.CategoryId,
            cancellationToken: cancellationToken);

        if (nameTaken)
            throw new InvalidOperationException(
                $"Another category named '{request.CategoryName}' already exists.");

        // 3. Apply changes
        category.CategoryName = request.CategoryName;
        category.IsApprovalRequired = request.IsApprovalRequired;
        category.UpdatedOn = DateTime.UtcNow;

        // 4. Mark dirty
        _categoryRepository.Update(category);

        // 5. Commit
        await _categoryRepository.SaveChangesAsync(cancellationToken);
    }
}