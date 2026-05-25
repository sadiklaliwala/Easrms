namespace Easrms.Application.DTOs.Auth;

public class OAuthUserInfo
{
    public string Email { get; set; } = string.Empty;
    public string ExternalUserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
