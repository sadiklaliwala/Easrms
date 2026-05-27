using Easrms.Common.Response;
using MediatR;

namespace Easrms.Application.Features.Auth.Commands.SendOtp;

public class SendOtpCommand : IRequest<ApiResponse<string>>
{
    public string Email { get; set; } = string.Empty;
}
