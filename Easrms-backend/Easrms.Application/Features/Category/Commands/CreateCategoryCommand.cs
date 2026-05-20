using Easrms.Application.Interfaces.Repositories;
using Easrms.Domain.Entities;
using MediatR;

namespace Easrms.Application.Features.Category.Commands;

/// <summary>
/// Creates a new request category.
/// Only Admin can dispatch this command (enforced at controller level).
/// </summary>
public sealed class CreateCategoryCommand : IRequest<Guid>
{
    public string CategoryName { get; init; } = string.Empty;
    public bool IsApprovalRequired { get; init; }

    public int SLAHours { get; set; }
}


/// <summary>
/// Step-by-step per HANDLER_REPO_REFERENCE_MAP:
///   1. IsCategoryNameTakenAsync(categoryName, excludeCategoryId: null) → 409 if true
///   2. Construct RequestCategory entity — IsActive = true, CreatedOn = UtcNow
///   3. AddAsync(entity)
///   4. SaveChangesAsync()
///   5. Return new CategoryId (201 set by controller)
/// </summary>
public sealed class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Guid>
{
    private readonly ICategoryRepository _categoryRepository;

    public CreateCategoryCommandHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Guid> Handle(
        CreateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Duplicate name check — no excludeId on create
        var nameTaken = await _categoryRepository.IsCategoryNameTakenAsync(
            request.CategoryName,
            excludeCategoryId: null,
            cancellationToken: cancellationToken);

        if (nameTaken)
            throw new InvalidOperationException(
                $"A category named '{request.CategoryName}' already exists.");

        if (request.SLAHours <= 0)
            throw new InvalidOperationException("SLA hours must be positive.");

        // 2. Build entity
        var category = new RequestCategory
        {
            CategoryId = Guid.NewGuid(),
            CategoryName = request.CategoryName,
            IsApprovalRequired = request.IsApprovalRequired,
            IsActive = true,
            CreatedOn = DateTime.UtcNow,
            SLAHours = request.SLAHours,
        };

        // 3. Persist
        await _categoryRepository.AddAsync(category, cancellationToken);

        // 4. Commit
        await _categoryRepository.SaveChangesAsync(cancellationToken);

        // 5. Return new id — controller wraps in 201 ApiResponse
        return category.CategoryId;
    }
}