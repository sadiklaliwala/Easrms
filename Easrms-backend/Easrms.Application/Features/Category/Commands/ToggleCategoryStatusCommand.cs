using Easrms.Application.Interfaces.Repositories;
using MediatR;

namespace Easrms.Application.Features.Category.Commands;

/// <summary>
/// Flips the IsActive flag of a category.
/// Only Admin can dispatch this command (enforced at controller level).
/// </summary>
public sealed class ToggleCategoryStatusCommand : IRequest<bool>
{
    /// <summary>Id of the category to toggle.</summary>
    public Guid CategoryId { get; init; }
}



/// <summary>
/// Step-by-step per HANDLER_REPO_REFERENCE_MAP:
///   1. GetByIdAsync(categoryId, trackChanges: true) → 404 if null
///   2. Flip entity.IsActive, set UpdatedOn = UtcNow
///   3. Update(entity)
///   4. SaveChangesAsync()
///   Returns the new IsActive value so the controller can write a meaningful message.
/// </summary>
public sealed class ToggleCategoryStatusCommandHandler : IRequestHandler<ToggleCategoryStatusCommand, bool>
{
    private readonly ICategoryRepository _categoryRepository;

    public ToggleCategoryStatusCommandHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<bool> Handle(
        ToggleCategoryStatusCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Fetch with tracking so EF detects the change
        var category = await _categoryRepository.GetByIdAsync(
            request.CategoryId,
            cancellationToken: cancellationToken)
            ?? throw new KeyNotFoundException(
                $"Category with id '{request.CategoryId}' was not found.");

        // 2. Flip and timestamp
        category.IsActive = !category.IsActive;
        category.UpdatedOn = DateTime.UtcNow;

        // 3. Mark dirty
        _categoryRepository.Update(category);

        // 4. Commit
        await _categoryRepository.SaveChangesAsync(cancellationToken);

        // Return new state — controller uses it for the response message
        return category.IsActive;
    }
}