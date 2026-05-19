using Easrms.Application.DTOs.Auth;
using Easrms.Application.Interfaces;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Common.Helpers;
using MediatR;

namespace Easrms.Application.Features.Auth.Commands;

public sealed class LoginCommand : IRequest<LoginResponseDto>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}


/// <summary>
/// 1. GetByEmailAsync(email, trackChanges: false)  → 401 if null
/// 2. entity.IsActive check                        → 403 if false
/// 3. PasswordHelper.Verify(password, hash)        → 401 if mismatch
/// 4. UpdateLastLoginAsync(userId)                 → ExecuteUpdateAsync, no SaveChanges
/// 5. Return LoginResponseDto — tokens set by AuthController
/// </summary>
public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IJwtSettings _jwtSettings;


    public LoginCommandHandler(IUserRepository userRepository , IJwtService jwtService , IJwtSettings jwtSettings)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _jwtSettings = jwtSettings;
    }

    public async Task<LoginResponseDto> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Fetch user by email
        var user = await _userRepository.GetByEmailAsync(
            request.Email,
            trackChanges: false,
            cancellationToken: cancellationToken
         ) ?? throw new UnauthorizedAccessException("Invalid email or password.");

        // 2. Active check
        if (!user.IsActive)
            throw new UnauthorizedAccessException(
                "Your account is inactive. Contact your administrator.");

        // 3. Password verification
        if (!PasswordHelper.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        //4. genrate AccessToken and RefreshToken here, so that we can set them in the response DTO before returning to controller. This way, we avoid having to set them in the controller later
        //and keeps the logic related to token generation within the handler where it belongs.
        
        var AccessToken = _jwtService.GenerateAccessToken(user);
        var RefreshToken = _jwtService.GenerateRefreshToken();
        _jwtService.SetTokenCookie(AccessToken);
        user.RefreshToken = RefreshToken;
        user.RefreshTokenExpiryOn = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays);
        // 5. Stamp LastLoginOn — direct ExecuteUpdateAsync, no SaveChanges needed
        await _userRepository.UpdateRefreshTokenAsync(user.UserId, RefreshToken, DateTime.Now.AddDays(_jwtSettings.RefreshTokenExpiryDays));
        await _userRepository.UpdateLastLoginAsync(user.UserId, cancellationToken);
        // 6. Map → DTO. AccessToken + RefreshToken populated by controller.

        return new LoginResponseDto
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            RoleName = user.Role.RoleName,
            ManagerId = user.ManagerId,
            //AccessToken = AccessToken,
            //RefreshToken = RefreshToken
        };
    }
}