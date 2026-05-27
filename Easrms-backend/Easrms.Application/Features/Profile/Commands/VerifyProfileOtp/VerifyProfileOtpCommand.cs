using Easrms.Common.Response;
using MediatR;

namespace Easrms.Application.Features.Profile.Commands.VerifyProfileOtp;

public class VerifyProfileOtpCommand : IRequest<ApiResponse<Easrms.Application.DTOs.Profile.VerifyProfileOtpResponseDto>>
{
    public Guid UserId { get; set; }
    public string OtpCode { get; set; } = string.Empty;
}
