using Easrms.Application.DTOs.Auth;
using Easrms.Application.Interfaces.OAuth;
using Easrms.Common.Constants;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Easrms.Infrastructure.Services.OAuth;

public class GoogleOAuthService : IOAuthService
{
    private readonly IConfiguration _config;
    private readonly ILogger<GoogleOAuthService> _logger;

    public GoogleOAuthService(IConfiguration config, ILogger<GoogleOAuthService> logger)
    {
        _config = config;
        _logger = logger;
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

        _logger.LogInformation("Starting Google authentication");


        var resp = await http.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(tokenRequest), cancellationToken);

        var responseBody = await resp.Content.ReadAsStringAsync(cancellationToken);

        _logger.LogInformation(
            "Google Response Status: {StatusCode}",
            resp.StatusCode
        );

        _logger.LogInformation(
            "Google Response Body: {ResponseBody}",
            responseBody
        );

        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogError(
                "Google authentication failed. Status: {StatusCode}, Response: {ResponseBody}",
                resp.StatusCode,
                responseBody
            );

            throw new UnauthorizedAccessException(
                $"Status: {resp.StatusCode} Google Related${responseBody} Error Occurred Please Login Again"
            );
        }

        using var doc = System.Text.Json.JsonDocument.Parse(responseBody);
        var root = doc.RootElement;
        if (!root.TryGetProperty("id_token", out var idTokenEl))
        {
            _logger.LogError("No id_token received from Google. Response: {ResponseBody}", responseBody);
            throw new UnauthorizedAccessException("No id_token received from Google.");
        }

        var payload = await GoogleJsonWebSignature.ValidateAsync(idTokenEl.GetString()!);

        return new OAuthUserInfo
        {
            Email = payload.Email ?? string.Empty,
            ExternalUserId = payload.Subject ?? string.Empty,
            Name = payload.Name ?? string.Empty
        };
    }
}
