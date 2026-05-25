using Easrms.Common.Constants;

namespace Easrms.Application.DTOs.Auth;

public class LinkProviderDto
{
    public string Code { get; set; } = string.Empty;
    public AuthProviderEnum Provider { get; set; }
}
