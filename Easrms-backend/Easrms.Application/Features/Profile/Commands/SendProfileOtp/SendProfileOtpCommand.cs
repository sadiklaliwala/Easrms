using Easrms.Common.Response;
using MediatR;

namespace Easrms.Application.Features.Profile.Commands.SendProfileOtp;

public class SendProfileOtpCommand : IRequest<ApiResponse<Easrms.Application.DTOs.Profile.SendProfileOtpResponseDto>>
{
    public Guid UserId { get; set; }
}
