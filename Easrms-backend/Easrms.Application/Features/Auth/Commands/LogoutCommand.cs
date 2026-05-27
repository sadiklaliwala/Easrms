using Easrms.Application.Interfaces.Jwt;
using Easrms.Application.Interfaces.Repositories;
using MediatR;

namespace Easrms.Application.Features.Auth.Commands;

/// <summary>
/// HttpOnly cookie removal is handled by the controller after this returns.
/// </summary>
public sealed class LogoutCommand : IRequest
{
    public Guid CurrentUserId { get; init; }
}


/// <summary>
/// 1. GetByIdAsync(currentUserId, trackChanges: false) → 401 if null
/// 2. RevokeRefreshTokenAsync(userId)                  → ExecuteUpdateAsync, no SaveChanges
/// 3. HttpOnly cookie cleared by AuthController
/// </summary>
public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public LogoutCommandHandler(IUserRepository userRepository , IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;

    }

    public async Task Handle(
        LogoutCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Confirm user exists
        var user = await _userRepository.GetByIdAsync(
            request.CurrentUserId,
            trackChanges: false,
            cancellationToken: cancellationToken);

        if (user is null)
            throw new UnauthorizedAccessException("User session not found.");

        // 2. Wipe refresh token — direct ExecuteUpdateAsync, no SaveChanges needed
        await _userRepository.RevokeRefreshTokenAsync(request.CurrentUserId, cancellationToken);

        // 3. Cookie removed by AuthController after this returns
        _jwtService.ClearTokenCookie();
        _jwtService.ClearRefreshTokenCookie();
    }
}