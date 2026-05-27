using Easrms.Application.Interfaces.Repositories;
using Easrms.Common.Response;
using Easrms.Common.Helpers;
using MediatR;
using Easrms.Application.Interfaces.Jwt;

namespace Easrms.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ApiResponse<string>>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public ResetPasswordCommandHandler(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public async Task<ApiResponse<string>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var principal = _jwtService.ValidatePasswordToken(request.PasswordResetToken, "password-reset");
        if (principal == null)
            return ApiResponse<string>.FailResponse("Invalid or expired token", 400);

        var userIdClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return ApiResponse<string>.FailResponse("Invalid token", 400);

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || !string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase))
            return ApiResponse<string>.FailResponse("Invalid token or email mismatch", 400);

        if (request.NewPassword != request.ConfirmPassword)
            return ApiResponse<string>.FailResponse("Password confirmation does not match", 400);

        user.PasswordHash = PasswordHelper.Hash(request.NewPassword);
        await _userRepository.UpdateAsync(user);

        return ApiResponse<string>.SuccessResponse(null, "Password has been reset.");
    }
}
