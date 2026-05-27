using Easrms.Application.Interfaces.Repositories;
using Easrms.Common.Response;
using Easrms.Common.Helpers;
using MediatR;
using Easrms.Application.Interfaces.Email;
using Easrms.Application.EmailTamplate;

namespace Easrms.Application.Features.Profile.Commands.SendProfileOtp;

public class SendProfileOtpCommandHandler : IRequestHandler<SendProfileOtpCommand, ApiResponse<Easrms.Application.DTOs.Profile.SendProfileOtpResponseDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;

    public SendProfileOtpCommandHandler(IUserRepository userRepository, IEmailService emailService)
    {
        _userRepository = userRepository;
        _emailService = emailService;
    }

    public async Task<ApiResponse<Easrms.Application.DTOs.Profile.SendProfileOtpResponseDto>> Handle(SendProfileOtpCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
            return ApiResponse<Easrms.Application.DTOs.Profile.SendProfileOtpResponseDto>.FailResponse("User not found", 400);

        var otp = Random.Shared.Next(100000, 999999).ToString();
        var hashed = PasswordHelper.Hash(otp);

        await _userRepository.UpdateUserOtpAsync(user.UserId, hashed, DateTime.UtcNow.AddMinutes(10));

        var subject = EmailTemplates.PasswordResetOtpSubject();
        var body = EmailTemplates.PasswordResetOtpBody(user.FullName, otp);

        await _emailService.SendAsync(user.Email, subject, body);

        var dto = new Easrms.Application.DTOs.Profile.SendProfileOtpResponseDto { Message = "OTP sent to your registered email" };
        return ApiResponse<Easrms.Application.DTOs.Profile.SendProfileOtpResponseDto>.SuccessResponse(dto, "OTP sent");
    }
}
