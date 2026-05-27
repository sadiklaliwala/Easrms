using Easrms.Common.Response;
using MediatR;

namespace Easrms.Application.Features.Request.Commands.ReopenRequest;

public class ReopenRequestCommand : IRequest<ApiResponse<string>>
{
    public Guid RequestId { get; set; }
    public Guid ReopenedBy { get; set; }
    public string ReopenReason { get; set; } = string.Empty;
}
