using Easrms.Application.DTOs.Common;
using Easrms.Application.Features.Category.Commands.BulkCreateCategories;
using Easrms.Application.Interfaces;
using Easrms.Common.Response;
using MediatR;

namespace Easrms.Application.Features.Category.Commands.BulkCreateCategories;

public class BulkCreateCategoriesHandler : IRequestHandler<BulkCreateCategoriesCommand, ApiResponse<BulkUploadResultDto>>
{
    private readonly ICategoryBulkUploadService _service;

    public BulkCreateCategoriesHandler(ICategoryBulkUploadService service)
    {
        _service = service;
    }

    public async Task<ApiResponse<BulkUploadResultDto>> Handle(BulkCreateCategoriesCommand request, CancellationToken cancellationToken)
    {
        using var ms = new MemoryStream(request.FileContent ?? Array.Empty<byte>());
        var result = await _service.ProcessAsync(ms, request.FileName, cancellationToken);
        return ApiResponse<BulkUploadResultDto>.SuccessResponse(result, "Bulk upload completed");
    }
}
