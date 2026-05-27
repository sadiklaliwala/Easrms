using Easrms.Application.DTOs.Auth;
using Easrms.Application.Interfaces.Jwt;
using Easrms.Application.Interfaces.OAuth;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Common.Constants;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Easrms.Application.Features.Auth.Commands.OAuthLogin;

public class OAuthLoginCommandHandler : IRequestHandler<OAuthLoginCommand, LoginResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthProviderRepository _authRepo;
    private readonly IJwtService _jwtService;
    private readonly IJwtSettings _jwtSettings;
    private readonly IEnumerable<IOAuthService> _oauthServices;

    public OAuthLoginCommandHandler(IEnumerable<IOAuthService> oauthServices,IUserRepository userRepository, IAuthProviderRepository authRepo, IJwtService jwtService, IJwtSettings jwtSettings)
    {
        _userRepository = userRepository;
        _authRepo = authRepo;
        _jwtService = jwtService;
        _jwtSettings = jwtSettings;
        _oauthServices = oauthServices;
    }

    public async Task<LoginResponseDto> Handle(OAuthLoginCommand request, CancellationToken cancellationToken)
    {
        IOAuthService oauthService = GetServiceForProvider(request.Dto.Provider);
        var userInfo = await oauthService.GetUserInfoAsync(request.Dto.Code, cancellationToken);

        var user = await _userRepository.GetByEmailAsync(userInfo.Email, trackChanges: false, cancellationToken: cancellationToken);
        if (user == null)
            throw new UnauthorizedAccessException("User not registered, contact Admin");

        var providerRow = await _authRepo.GetByUserIdAndProviderAsync(user.UserId, request.Dto.Provider);
        if (providerRow == null)
            throw new UnauthorizedAccessException("Provider not linked Plz Login Normally and Link from Profile Section");

        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();
        _jwtService.SetTokenCookie(accessToken);
        _jwtService.SetRefreshTokenCookie(refreshToken);

        await _userRepository.UpdateLoginMetaAsync(user.UserId, refreshToken, DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays), cancellationToken);

        return new LoginResponseDto
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            RoleName = user.Role.RoleName,
            ManagerId = user.ManagerId
        };
    }

    private IOAuthService GetServiceForProvider(AuthProviderEnum provider)
    {
        return _oauthServices.FirstOrDefault(x => x.Provider == provider)
            ?? throw new InvalidOperationException("Unsupported provider");
    }
}
