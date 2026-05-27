using Easrms.Application.Interfaces.Repositories;
using Easrms.Common.Response;
using Easrms.Common.Enums;
using MediatR;

namespace Easrms.Application.Features.Category.Commands.DeleteCategory;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, ApiResponse<string>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IRequestRepository _requestRepository;

    public DeleteCategoryCommandHandler(ICategoryRepository categoryRepository, IRequestRepository requestRepository)
    {
        _categoryRepository = categoryRepository;
        _requestRepository = requestRepository;
    }

    public async Task<ApiResponse<string>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category == null)
            return ApiResponse<string>.FailResponse("Category not found.", 404);

        var hasActive = await _requestRepository.HasActiveRequestsForCategoryAsync(request.CategoryId, cancellationToken);
        if (hasActive)
            return ApiResponse<string>.FailResponse("Category cannot be deleted as it has active requests.", 409);

        var ok = await _categoryRepository.SoftDeleteCategoryAsync(request.CategoryId, cancellationToken);
        if (!ok)
            return ApiResponse<string>.FailResponse("Failed to delete category.", 500);

        return ApiResponse<string>.SuccessResponse(null, "Category deleted successfully.");
    }
}
