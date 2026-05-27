using MediatR;
using Easrms.Common.Response;

namespace Easrms.Application.Features.Request.Commands.BulkCreateRequests;

public class BulkCreateRequestsCommand : IRequest<ApiResponse<Easrms.Application.DTOs.Common.BulkUploadResultDto>>
{
    public string FileName { get; set; } = string.Empty;
    public byte[] FileContent { get; set; } = Array.Empty<byte>();
    public Guid EmployeeId { get; set; }
}
