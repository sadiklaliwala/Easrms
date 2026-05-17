using Easrms.Application.DTOs.Auth;
using Easrms.Application.Interfaces.Repositories;
using MediatR;

namespace Easrms.Application.Features.Auth.Commands;

public sealed class RefreshTokenCommand : IRequest<RefreshTokenResponseDto>
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
}


/// <summary>
/// 1. GetByRefreshTokenAsync(token, trackChanges: false)
///    → null          = 401 invalid token
///    → token expired = 401 token expired
/// 2. Handler returns shell DTO — controller generates new JWT + RefreshToken,
///    calls UpdateRefreshTokenAsync, then patches the response.
/// </summary>
public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResponseDto>
{
    private readonly IUserRepository _userRepository;

    public RefreshTokenCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<RefreshTokenResponseDto> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        // 1a. Resolve user by refresh token
        var user = await _userRepository.GetByRefreshTokenAsync(
            request.RefreshToken,
            trackChanges: false,
            cancellationToken: cancellationToken);

        if (user is null)
            throw new UnauthorizedAccessException("Invalid refresh token.");

        // 1b. Expiry check
        if (user.RefreshTokenExpiryOn <= DateTime.UtcNow)
            throw new UnauthorizedAccessException(
                "Refresh token has expired. Please log in again.");

        // Controller generates new tokens, calls UpdateRefreshTokenAsync, then fills this DTO.
        return new RefreshTokenResponseDto
        {
            AccessToken = string.Empty,   // set by controller
            RefreshToken = string.Empty    // set by controller
        };
    }
}