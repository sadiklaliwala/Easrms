using Easrms.Application.Interfaces.Repositories;
using Easrms.Common.Response;
using Easrms.Common.Helpers;
using MediatR;
using Easrms.Application.Interfaces.Jwt;

namespace Easrms.Application.Features.Auth.Commands.VerifyOtp;

public class VerifyOtpCommandHandler : IRequestHandler<VerifyOtpCommand, ApiResponse<Easrms.Application.DTOs.Auth.VerifyOtpResponseDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public VerifyOtpCommandHandler(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public async Task<ApiResponse<Easrms.Application.DTOs.Auth.VerifyOtpResponseDto>> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailForOtpAsync(request.Email);
        if (user == null)
            return ApiResponse<Easrms.Application.DTOs.Auth.VerifyOtpResponseDto>.FailResponse("Invalid OTP", 400);

        if (!user.OtpExpiryOn.HasValue || user.OtpExpiryOn.Value < DateTime.UtcNow)
            return ApiResponse<Easrms.Application.DTOs.Auth.VerifyOtpResponseDto>.FailResponse("OTP has expired", 400);

        var valid = PasswordHelper.Verify(request.OtpCode, user.OtpCode ?? string.Empty);
        if (!valid)
            return ApiResponse<Easrms.Application.DTOs.Auth.VerifyOtpResponseDto>.FailResponse("Invalid OTP", 400);

        await _userRepository.UpdateUserOtpAsync(user.UserId, null, null);

        var token = _jwtService.GeneratePasswordToken(user.UserId, "password-reset");

        var dto = new Easrms.Application.DTOs.Auth.VerifyOtpResponseDto { PasswordResetToken = token };

        return ApiResponse<Easrms.Application.DTOs.Auth.VerifyOtpResponseDto>.SuccessResponse(dto, "OTP verified.");
    }
}
