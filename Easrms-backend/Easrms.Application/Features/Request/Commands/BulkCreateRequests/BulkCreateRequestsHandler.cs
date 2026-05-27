using Easrms.Application.DTOs.Common;
using Easrms.Application.Features.Request.Commands.BulkCreateRequests;
using Easrms.Application.Interfaces;
using Easrms.Common.Response;
using MediatR;

namespace Easrms.Application.Features.Request.Commands.BulkCreateRequests;

public class BulkCreateRequestsHandler : IRequestHandler<BulkCreateRequestsCommand, ApiResponse<BulkUploadResultDto>>
{
    private readonly IRequestBulkUploadService _service;

    public BulkCreateRequestsHandler(IRequestBulkUploadService service)
    {
        _service = service;
    }

    public async Task<ApiResponse<BulkUploadResultDto>> Handle(BulkCreateRequestsCommand request, CancellationToken cancellationToken)
    {
        using var ms = new MemoryStream(request.FileContent ?? Array.Empty<byte>());
        var result = await _service.ProcessAsync(ms, request.FileName, request.EmployeeId, cancellationToken);
        return ApiResponse<BulkUploadResultDto>.SuccessResponse(result, "Bulk upload completed");
    }
}
