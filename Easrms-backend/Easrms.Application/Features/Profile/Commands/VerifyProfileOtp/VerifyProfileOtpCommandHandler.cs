using Easrms.Application.DTOs.Profile;
using Easrms.Application.Interfaces.Jwt;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Common.Helpers;
using Easrms.Common.Response;
using MediatR;

namespace Easrms.Application.Features.Profile.Commands.VerifyProfileOtp;

public class VerifyProfileOtpCommandHandler : IRequestHandler<VerifyProfileOtpCommand, ApiResponse<Easrms.Application.DTOs.Profile.VerifyProfileOtpResponseDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public VerifyProfileOtpCommandHandler(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public async Task<ApiResponse<Easrms.Application.DTOs.Profile.VerifyProfileOtpResponseDto>> Handle(VerifyProfileOtpCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
            return ApiResponse<Easrms.Application.DTOs.Profile.VerifyProfileOtpResponseDto>.FailResponse("Invalid OTP", 400);

        if (!user.OtpExpiryOn.HasValue || user.OtpExpiryOn.Value < DateTime.UtcNow)
            return ApiResponse<Easrms.Application.DTOs.Profile.VerifyProfileOtpResponseDto>.FailResponse("OTP has expired", 400);

        var valid = PasswordHelper.Verify(request.OtpCode, user.OtpCode ?? string.Empty);
        if (!valid)
            return ApiResponse<VerifyProfileOtpResponseDto>.FailResponse("Invalid OTP", 400);

        await _userRepository.UpdateUserOtpAsync(user.UserId, null, null);

        var token = _jwtService.GeneratePasswordToken(user.UserId, "password-change");
        var dto = new VerifyProfileOtpResponseDto { PasswordChangeToken = token };

        return ApiResponse<VerifyProfileOtpResponseDto>.SuccessResponse(dto, "OTP verified.");
    }
}
