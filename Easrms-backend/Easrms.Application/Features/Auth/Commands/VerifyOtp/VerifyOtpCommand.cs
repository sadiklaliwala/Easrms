using Easrms.Common.Response;
using MediatR;

namespace Easrms.Application.Features.Auth.Commands.VerifyOtp;

public class VerifyOtpCommand : IRequest<ApiResponse<Easrms.Application.DTOs.Auth.VerifyOtpResponseDto>>
{
    public string Email { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
}
