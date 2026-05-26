using Easrms.Application.DTOs.Auth;
using Easrms.Application.Interfaces.OAuth;
using Easrms.Common.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using System.Text.Json;

namespace Easrms.Infrastructure.Services.OAuth;

public class AzureOAuthService : IOAuthService
{
    private readonly IConfiguration _config;

    public AzureOAuthService(IConfiguration config)
    {
        _config = config;
    }
    public AuthProviderEnum Provider => AuthProviderEnum.Azure;
    public async Task<OAuthUserInfo> GetUserInfoAsync(string code, CancellationToken cancellationToken = default)
    {
        var settings = _config.GetSection("OAuth:Azure");
        var clientId = settings["ClientId"];
        var clientSecret = settings["ClientSecret"];
        var redirectUri = settings["RedirectUri"];

        // Exchange code for token
        using var http = new HttpClient();
        var tokenRequest = new Dictionary<string, string>
        {
            {"client_id", clientId},
            {"client_secret", clientSecret},
            {"code", code},
            {"redirect_uri", redirectUri},
            {"grant_type", "authorization_code"},
            {"scope", "openid profile email"}
        };

        var tokenResp = await http.PostAsync("https://login.microsoftonline.com/common/oauth2/v2.0/token", new FormUrlEncodedContent(tokenRequest), cancellationToken);
        if (!tokenResp.IsSuccessStatusCode)
            throw new UnauthorizedAccessException("Failed to exchange code with Azure.");

        var tokenJson = await tokenResp.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(tokenJson);
        var root = doc.RootElement;
        if (!root.TryGetProperty("id_token", out var idTokenEl))
            throw new UnauthorizedAccessException("No id_token received from Azure.");

        var idToken = idTokenEl.GetString()!;
        // Decode JWT to get claims
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(idToken);
        var email = jwt.Claims.FirstOrDefault(c => c.Type == "email")?.Value
                    ?? jwt.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value
                    ?? string.Empty;
        var oid = jwt.Claims.FirstOrDefault(c => c.Type == "oid")?.Value ?? string.Empty;
        var name = jwt.Claims.FirstOrDefault(c => c.Type == "name")?.Value ?? string.Empty;

        return new OAuthUserInfo
        {
            Email = email,
            ExternalUserId = oid,
            Name = name
        };
    }
}
