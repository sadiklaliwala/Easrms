using Easrms.Application.DTOs.Auth;
using Easrms.Application.Interfaces.OAuth;
using Easrms.Common.Constants;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Easrms.Infrastructure.Services.OAuth;

public class LinkedInOAuthService : IOAuthService
{
    private readonly IConfiguration _config;

    public LinkedInOAuthService(IConfiguration config)
    {
        _config = config;
    }

    public AuthProviderEnum Provider => AuthProviderEnum.LinkedIn;

    public async Task<OAuthUserInfo> GetUserInfoAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        var settings = _config.GetSection("OAuth:LinkedIn");

        var clientId = settings["ClientId"];
        var clientSecret = settings["ClientSecret"];
        var redirectUri = settings["RedirectUri"];

        if (string.IsNullOrWhiteSpace(clientId) ||
            string.IsNullOrWhiteSpace(clientSecret) ||
            string.IsNullOrWhiteSpace(redirectUri))
        {
            throw new InvalidOperationException(
                "LinkedIn OAuth settings are missing.");
        }

        using var http = new HttpClient();

        // STEP 1: Exchange authorization code for access token
        var tokenRequest = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "code", code },
            { "redirect_uri", redirectUri },
            { "client_id", clientId },
            { "client_secret", clientSecret }
        };

        var tokenResp = await http.PostAsync(
            "https://www.linkedin.com/oauth/v2/accessToken",
            new FormUrlEncodedContent(tokenRequest),
            cancellationToken);

        var tokenResponseContent =
            await tokenResp.Content.ReadAsStringAsync(cancellationToken);

        if (!tokenResp.IsSuccessStatusCode)
        {
            throw new UnauthorizedAccessException(
                $"Failed to exchange code with LinkedIn. Response: {tokenResponseContent}");
        }

        using var tokenDoc = JsonDocument.Parse(tokenResponseContent);

        var tokenRoot = tokenDoc.RootElement;

        var accessToken =
            tokenRoot.TryGetProperty("access_token", out var tokenEl)
                ? tokenEl.GetString()
                : null;

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new UnauthorizedAccessException(
                "No access token returned from LinkedIn.");
        }

        // STEP 2: Set bearer token
        http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        http.DefaultRequestHeaders.UserAgent.ParseAdd("Easrms/1.0");

        // STEP 3: Fetch user profile from OpenID Connect endpoint
        var profileResp = await http.GetAsync(
            "https://api.linkedin.com/v2/userinfo",
            cancellationToken);

        var profileResponseContent =
            await profileResp.Content.ReadAsStringAsync(cancellationToken);

        if (!profileResp.IsSuccessStatusCode)
        {
            throw new UnauthorizedAccessException(
                $"Failed to fetch LinkedIn user profile. Response: {profileResponseContent}");
        }

        using var profileDoc = JsonDocument.Parse(profileResponseContent);

        var profileRoot = profileDoc.RootElement;

        // OpenID Connect fields
        var externalUserId =
            profileRoot.TryGetProperty("sub", out var subEl)
                ? subEl.GetString() ?? string.Empty
                : string.Empty;

        var name =
            profileRoot.TryGetProperty("name", out var nameEl)
                ? nameEl.GetString() ?? string.Empty
                : string.Empty;

        var email =
            profileRoot.TryGetProperty("email", out var emailEl)
                ? emailEl.GetString() ?? string.Empty
                : string.Empty;

        var givenName =
            profileRoot.TryGetProperty("given_name", out var givenNameEl)
                ? givenNameEl.GetString() ?? string.Empty
                : string.Empty;

        var familyName =
            profileRoot.TryGetProperty("family_name", out var familyNameEl)
                ? familyNameEl.GetString() ?? string.Empty
                : string.Empty;

        var picture =
            profileRoot.TryGetProperty("picture", out var pictureEl)
                ? pictureEl.GetString() ?? string.Empty
                : string.Empty;

        if (string.IsNullOrWhiteSpace(externalUserId))
        {
            throw new UnauthorizedAccessException(
                "LinkedIn did not return a valid user id.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new UnauthorizedAccessException(
                "LinkedIn did not return user email.");
        }

        // Fallback name creation
        if (string.IsNullOrWhiteSpace(name))
        {
            name = $"{givenName} {familyName}".Trim();
        }

        return new OAuthUserInfo
        {
            ExternalUserId = externalUserId,
            Email = email,
            Name = name
        };
    }
}