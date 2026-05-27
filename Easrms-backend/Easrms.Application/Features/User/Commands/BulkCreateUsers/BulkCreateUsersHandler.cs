using Easrms.Application.DTOs.Common;
using Easrms.Application.Features.User.Commands.BulkCreateUsers;
using Easrms.Application.Interfaces;
using Easrms.Common.Response;
using MediatR;

namespace Easrms.Application.Features.User.Commands.BulkCreateUsers;

public class BulkCreateUsersHandler : IRequestHandler<BulkCreateUsersCommand, ApiResponse<BulkUploadResultDto>>
{
    private readonly IUserBulkUploadService _service;

    public BulkCreateUsersHandler(IUserBulkUploadService service)
    {
        _service = service;
    }

    public async Task<ApiResponse<BulkUploadResultDto>> Handle(BulkCreateUsersCommand request, CancellationToken cancellationToken)
    {
        using var ms = new MemoryStream(request.FileContent ?? Array.Empty<byte>());
        var result = await _service.ProcessAsync(ms, request.FileName, cancellationToken);
        return ApiResponse<BulkUploadResultDto>.SuccessResponse(result, "Bulk upload completed");
    }
}
