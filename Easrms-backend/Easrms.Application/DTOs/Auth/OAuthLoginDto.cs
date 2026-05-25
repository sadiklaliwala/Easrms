using Easrms.Common.Constants;

namespace Easrms.Application.DTOs.Auth;

public class OAuthLoginDto
{
    public string Code { get; set; } = string.Empty;
    public AuthProviderEnum Provider { get; set; }
}
