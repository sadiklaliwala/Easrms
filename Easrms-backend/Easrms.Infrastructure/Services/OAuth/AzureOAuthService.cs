using Easrms.Application.DTOs.Auth;
using Easrms.Application.Interfaces.OAuth;
using Easrms.Common.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using System.Text.Json;

namespace Easrms.Infrastructure.Services.OAuth;

public class AzureOAuthService : IOAuthService
{
    private readonly IConfiguration _config;
    private readonly ILogger<AzureOAuthService> _logger;

    public AzureOAuthService(IConfiguration config, ILogger<AzureOAuthService> logger)
    {
        _config = config;
        _logger = logger;
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

        _logger.LogInformation("Starting Azure authentication");

        var tokenResp = await http.PostAsync("https://login.microsoftonline.com/common/oauth2/v2.0/token", new FormUrlEncodedContent(tokenRequest), cancellationToken);
        _logger.LogInformation("Azure Token Response Status: {StatusCode}", tokenResp.StatusCode);

        var tokenJson = await tokenResp.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogInformation("Azure Token Response Body: {ResponseBody}", tokenJson);

        if (!tokenResp.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to exchange code with Azure. Status: {StatusCode}, Response: {ResponseBody}", tokenResp.StatusCode, tokenJson);
            throw new UnauthorizedAccessException("Failed to exchange code with Azure.");
        }

        using var doc = JsonDocument.Parse(tokenJson);
        var root = doc.RootElement;
        if (!root.TryGetProperty("id_token", out var idTokenEl))
        {
            _logger.LogError("No id_token received from Azure. Response: {ResponseBody}", tokenJson);
            throw new UnauthorizedAccessException("No id_token received from Azure.");
        }

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
