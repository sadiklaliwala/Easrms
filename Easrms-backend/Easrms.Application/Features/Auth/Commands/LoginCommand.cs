using Easrms.Application.DTOs.Auth;
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

    public LoginCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<LoginResponseDto> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Fetch user by email
        var user = await _userRepository.GetByEmailAsync(
            request.Email,
            trackChanges: false,
            cancellationToken: cancellationToken);

        if (user is null)
            throw new UnauthorizedAccessException("Invalid email or password.");

        // 2. Active check
        if (!user.IsActive)
            throw new UnauthorizedAccessException(
                "Your account is inactive. Contact your administrator.");

        // 3. Password verification
        if (!PasswordHelper.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        // 4. Stamp LastLoginOn — direct ExecuteUpdateAsync, no SaveChanges needed
        await _userRepository.UpdateLastLoginAsync(user.UserId, cancellationToken);

        // 5. Map → DTO. AccessToken + RefreshToken populated by controller.
        return new LoginResponseDto
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            RoleName = user.Role.RoleName,
            ManagerId = user.ManagerId,
            AccessToken = string.Empty,   // set by controller
            RefreshToken = string.Empty    // set by controller
        };
    }
}