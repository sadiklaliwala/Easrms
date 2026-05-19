using Easrms.Application.DTOs.Auth;
using Easrms.Application.Interfaces;
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
    private readonly IJwtService _jwtService;

    public RefreshTokenCommandHandler(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
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

        //4. genrate AccessToken and RefreshToken here, so that we can set them in the response DTO before returning to controller. This way, we avoid having to set them in the controller later
        //and keeps the logic related to token generation within the handler where it belongs.

        var AccessToken = _jwtService.GenerateAccessToken(user);
        var RefreshToken = _jwtService.GenerateRefreshToken();


        return new RefreshTokenResponseDto
        {
            AccessToken = AccessToken,
            RefreshToken = RefreshToken
        };
    }
}