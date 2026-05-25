using Easrms.Application.DTOs.Auth;
using Easrms.Application.Interfaces.OAuth;
using Easrms.Common.Constants;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Easrms.Infrastructure.Services.OAuth;

public class GitHubOAuthService : IOAuthService
{
    private readonly IConfiguration _config;

    public GitHubOAuthService(IConfiguration config)
    {
        _config = config;
    }
    public AuthProviderEnum Provider => AuthProviderEnum.GitHub;
    public async Task<OAuthUserInfo> GetUserInfoAsync(string code, CancellationToken cancellationToken = default)
    {
        var settings = _config.GetSection("OAuth:GitHub");
        var clientId = settings["ClientId"];
        var clientSecret = settings["ClientSecret"];
        var redirectUri = settings["RedirectUri"];

        using var http = new HttpClient();
        var tokenRequest = new Dictionary<string, string>
        {
            {"client_id", clientId},
            {"client_secret", clientSecret},
            {"code", code},
            {"redirect_uri", redirectUri}
        };

        var tokenResp = await http.PostAsync("https://github.com/login/oauth/access_token", new FormUrlEncodedContent(tokenRequest), cancellationToken);
        var tokenJson = await tokenResp.Content.ReadAsStringAsync(cancellationToken);
        // token response is x-www-form-urlencoded
        var parsed = System.Web.HttpUtility.ParseQueryString(tokenJson);
        var accessToken = parsed["access_token"];
        if (string.IsNullOrEmpty(accessToken))
            throw new UnauthorizedAccessException("Failed to obtain GitHub access token.");

        http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Easrms", "1.0"));
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Get user
        var userResp = await http.GetAsync("https://api.github.com/user", cancellationToken);
        if (!userResp.IsSuccessStatusCode)
            throw new UnauthorizedAccessException("Failed to fetch GitHub user.");

        var userJson = await userResp.Content.ReadAsStringAsync(cancellationToken);
        using var userDoc = JsonDocument.Parse(userJson);
        var userRoot = userDoc.RootElement;
        var id = userRoot.GetProperty("id").GetInt64().ToString();
        var name = userRoot.TryGetProperty("name", out var n) ? n.GetString() ?? string.Empty : string.Empty;

        // Get emails
        var emailsResp = await http.GetAsync("https://api.github.com/user/emails", cancellationToken);
        if (!emailsResp.IsSuccessStatusCode)
            throw new UnauthorizedAccessException("Failed to fetch GitHub user emails.");

        var emailsJson = await emailsResp.Content.ReadAsStringAsync(cancellationToken);
        using var emailsDoc = JsonDocument.Parse(emailsJson);
        string? primaryEmail = null;
        foreach (var el in emailsDoc.RootElement.EnumerateArray())
        {
            var primary = el.GetProperty("primary").GetBoolean();
            var verified = el.GetProperty("verified").GetBoolean();
            if (primary && verified)
            {
                primaryEmail = el.GetProperty("email").GetString();
                break;
            }
        }

        if (string.IsNullOrEmpty(primaryEmail))
            throw new UnauthorizedAccessException("No verified primary email found for GitHub user.");

        return new OAuthUserInfo
        {
            Email = primaryEmail,
            ExternalUserId = id,
            Name = name
        };
    }
}
