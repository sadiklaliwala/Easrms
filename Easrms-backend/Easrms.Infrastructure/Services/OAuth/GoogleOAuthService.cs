using Easrms.Application.DTOs.Auth;
using Easrms.Application.Interfaces.OAuth;
using Easrms.Common.Constants;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Easrms.Infrastructure.Services.OAuth;

public class GoogleOAuthService : IOAuthService
{
    private readonly IConfiguration _config;

    public GoogleOAuthService(IConfiguration config)
    {
        _config = config;
    }

    public AuthProviderEnum Provider => AuthProviderEnum.Google;

    public async Task<OAuthUserInfo> GetUserInfoAsync(string code, CancellationToken cancellationToken = default)
    {
        // Note: Implementing full OAuth token exchange requires HTTP calls to Google's token endpoint.
        // For brevity and to avoid heavy dependencies here, we'll perform a simple token verification flow
        // assuming the client will exchange code for id_token on the client side and pass id_token instead.
        // However following the spec, we'll attempt to exchange code via token endpoint.

        var googleSettings = _config.GetSection("OAuth:Google");
        var clientId = googleSettings["ClientId"];
        var clientSecret = googleSettings["ClientSecret"];
        var redirectUri = googleSettings["RedirectUri"];

        // Exchange code for tokens
        using var http = new HttpClient();
        var tokenRequest = new Dictionary<string, string>
        {
            {"code", code},
            {"client_id", clientId},
            {"client_secret", clientSecret},
            {"redirect_uri", redirectUri},
            {"grant_type", "authorization_code"}
        };

        var resp = await http.PostAsync("https://oauth2.googleapis.com/token",new FormUrlEncodedContent(tokenRequest),cancellationToken);

        var responseBody = await resp.Content.ReadAsStringAsync(cancellationToken);

        if (!resp.IsSuccessStatusCode)
        {
            throw new UnauthorizedAccessException(
                $"Google token exchange failed.\n" +
                $"Status: {resp.StatusCode}\n" +
                $"Body: {responseBody}");
        }

        //var json = await resp.Content.ReadAsStringAsync(cancellationToken);
        using var doc = System.Text.Json.JsonDocument.Parse(responseBody);
        var root = doc.RootElement;
        if (!root.TryGetProperty("id_token", out var idTokenEl))
            throw new UnauthorizedAccessException("No id_token received from Google.");

        var payload = await GoogleJsonWebSignature.ValidateAsync(idTokenEl.GetString()!);

        return new OAuthUserInfo
        {
            Email = payload.Email ?? string.Empty,
            ExternalUserId = payload.Subject ?? string.Empty,
            Name = payload.Name ?? string.Empty
        };
    }
}
