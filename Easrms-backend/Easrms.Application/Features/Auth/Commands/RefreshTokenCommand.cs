using Easrms.Application.DTOs.Auth;
using Easrms.Application.Interfaces;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Application.Settings;
using MediatR;

namespace Easrms.Application.Features.Auth.Commands;

public sealed class RefreshTokenCommand : IRequest<RefreshTokenResponseDto>
{
    //public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
}


public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IJwtSettings _jwtSettings;
    



    public RefreshTokenCommandHandler(IUserRepository userRepository, IJwtService jwtService, IJwtSettings jwtSettings)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _jwtSettings = jwtSettings;
    }

    public async Task<RefreshTokenResponseDto> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var refreshToken = _jwtService.GetRefreshTokenFromCookie();

        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new UnauthorizedAccessException("Refresh token not found.");

        var user = await _userRepository.GetByRefreshTokenAsync(
            refreshToken,
            trackChanges: false,
            cancellationToken: cancellationToken);

        if (user is null)
            throw new UnauthorizedAccessException("Invalid refresh token.");

        if (user.RefreshTokenExpiryOn <= DateTime.UtcNow)
            throw new UnauthorizedAccessException(
                "Refresh token has expired. Please log in again.");

        var AccessToken = _jwtService.GenerateAccessToken(user);
        var RefreshToken = _jwtService.GenerateRefreshToken();
        _jwtService.SetTokenCookie(AccessToken);
        _jwtService.SetRefreshTokenCookie(RefreshToken);
        await _userRepository.UpdateRefreshTokenAsync(
            user.UserId, RefreshToken,
            DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
            cancellationToken);

        return new RefreshTokenResponseDto
        {
            AccessToken = AccessToken,
            RefreshToken = RefreshToken
        };
    }
}