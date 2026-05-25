using Easrms.Application.DTOs.Auth;
using Easrms.Common.Constants;

namespace Easrms.Application.Interfaces.OAuth;

public interface IOAuthService
{
    AuthProviderEnum Provider { get; }
    Task<OAuthUserInfo> GetUserInfoAsync(string code, CancellationToken cancellationToken = default);
}
