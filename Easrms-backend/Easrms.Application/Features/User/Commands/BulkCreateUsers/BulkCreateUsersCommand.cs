using MediatR;
using Easrms.Common.Response;

namespace Easrms.Application.Features.User.Commands.BulkCreateUsers;

public class BulkCreateUsersCommand : IRequest<ApiResponse<Easrms.Application.DTOs.Common.BulkUploadResultDto>>
{
    public string FileName { get; set; } = string.Empty;
    public byte[] FileContent { get; set; } = Array.Empty<byte>();
}
