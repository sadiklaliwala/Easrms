using Easrms.Application.Interfaces.Repositories;
using Easrms.Common.Response;
using Easrms.Common.Helpers;
using MediatR;
using Easrms.Application.Interfaces.Jwt;

namespace Easrms.Application.Features.Profile.Commands.ChangePassword;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, ApiResponse<string>>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public ChangePasswordCommandHandler(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public async Task<ApiResponse<string>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var principal = _jwtService.ValidatePasswordToken(request.PasswordChangeToken, "password-change");
        if (principal == null)
            return ApiResponse<string>.FailResponse("Invalid or expired token", 400);

        var userIdClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var tokenUserId))
            return ApiResponse<string>.FailResponse("Invalid token", 400);

        if (tokenUserId != request.CurrentUserId)
            return ApiResponse<string>.FailResponse("Token user mismatch", 400);

        if (request.NewPassword != request.ConfirmPassword)
            return ApiResponse<string>.FailResponse("Password confirmation does not match", 400);

        var user = await _userRepository.GetByIdAsync(request.CurrentUserId);
        if (user == null)
            return ApiResponse<string>.FailResponse("User not found", 400);

        user.PasswordHash = PasswordHelper.Hash(request.NewPassword);
        await _userRepository.UpdateAsync(user);

        return ApiResponse<string>.SuccessResponse(null, "Password changed successfully.");
    }
}
