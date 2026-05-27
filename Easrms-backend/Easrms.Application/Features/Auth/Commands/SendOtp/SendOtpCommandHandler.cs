using Easrms.Application.Interfaces.Repositories;
using Easrms.Common.Response;
using Easrms.Common.Helpers;
using MediatR;
using Easrms.Application.Interfaces.Email;
using Easrms.Application.EmailTamplate;

namespace Easrms.Application.Features.Auth.Commands.SendOtp;

public class SendOtpCommandHandler : IRequestHandler<SendOtpCommand, ApiResponse<string>>
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;


    public SendOtpCommandHandler(IUserRepository userRepository, IEmailService emailService)
    {
        _userRepository = userRepository;
        _emailService = emailService;
    }

    public async Task<ApiResponse<string>> Handle(SendOtpCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailForOtpAsync(request.Email);

        // Always respond success to avoid user enumeration
        var successMessage = "If this email is registered, an OTP has been sent.";

        if (user == null)
            return ApiResponse<string>.SuccessResponse(null, successMessage);

        // Generate OTP and hash
        var otp = Random.Shared.Next(100000, 999999).ToString();
        var hashed = PasswordHelper.Hash(otp);

        await _userRepository.UpdateUserOtpAsync(user.UserId, hashed, DateTime.UtcNow.AddMinutes(10));

        // Send email using existing template
        var subject = EmailTemplates.PasswordResetOtpSubject();
        var body = EmailTemplates.PasswordResetOtpBody(user.FullName, otp);

        await _emailService.SendAsync(user.Email, subject, body);

        return ApiResponse<string>.SuccessResponse(null, successMessage);
    }
}
