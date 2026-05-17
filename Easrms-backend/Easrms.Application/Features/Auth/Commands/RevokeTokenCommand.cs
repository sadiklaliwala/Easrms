using Easrms.Application.Interfaces.Repositories;
using MediatR;

namespace Easrms.Application.Features.Auth.Commands;

public sealed class RevokeTokenCommand : IRequest
{
    public string RefreshToken { get; init; } = string.Empty;
}


/// <summary>
/// 1. GetByRefreshTokenAsync(token, trackChanges: false) → 401 if null
/// 2. RevokeRefreshTokenAsync(userId)                    → ExecuteUpdateAsync, no SaveChanges
/// </summary>
public sealed class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand>
{
    private readonly IUserRepository _userRepository;

    public RevokeTokenCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task Handle(
        RevokeTokenCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Resolve user — ensures token belongs to a real record
        var user = await _userRepository.GetByRefreshTokenAsync(
            request.RefreshToken,
            trackChanges: false,
            cancellationToken: cancellationToken);

        if (user is null)
            throw new UnauthorizedAccessException(
                "Refresh token is invalid or has already been revoked.");

        // 2. Wipe token — direct ExecuteUpdateAsync, no SaveChanges needed
        await _userRepository.RevokeRefreshTokenAsync(user.UserId, cancellationToken);
    }
}