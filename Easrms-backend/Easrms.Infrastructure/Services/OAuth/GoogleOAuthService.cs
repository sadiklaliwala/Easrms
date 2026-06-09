using Easrms.Application.DTOs.Auth;
using Easrms.Application.Interfaces.OAuth;
using Easrms.Common.Constants;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Easrms.Infrastructure.Services.OAuth;

public class GoogleOAuthService : IOAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GoogleOAuthService> _logger;

    // Config values read once at startup — fails fast if misconfigured
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _redirectUri;

    public GoogleOAuthService(
        IHttpClientFactory httpClientFactory,
        IConfiguration config,
        ILogger<GoogleOAuthService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("Google");
        _logger = logger;

        var googleSettings = config.GetSection("OAuth:Google");

        _clientId = googleSettings["ClientId"]
            ?? throw new InvalidOperationException("OAuth:Google:ClientId is not configured.");

        _clientSecret = googleSettings["ClientSecret"]
            ?? throw new InvalidOperationException("OAuth:Google:ClientSecret is not configured.");

        _redirectUri = googleSettings["RedirectUri"]
            ?? throw new InvalidOperationException("OAuth:Google:RedirectUri is not configured.");
    }

    public AuthProviderEnum Provider => AuthProviderEnum.Google;

    public async Task<OAuthUserInfo> GetUserInfoAsync(string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Authorization code must not be empty.", nameof(code));

        _logger.LogInformation("Starting Google OAuth token exchange.");

        // Step 1: Exchange authorization code for tokens
        var idToken = await ExchangeCodeForIdTokenAsync(code, cancellationToken);

        // Step 2: Validate id_token and extract user info
        var userInfo = await ValidateAndExtractUserInfoAsync(idToken, cancellationToken);

        _logger.LogInformation(
            "Google OAuth authentication succeeded for ExternalUserId: {ExternalUserId}",
            userInfo.ExternalUserId);

        return userInfo;
    }

    // -------------------------------------------------------------------------
    // Private Helpers
    // -------------------------------------------------------------------------

    private async Task<string> ExchangeCodeForIdTokenAsync(string code, CancellationToken cancellationToken)
    {
        var tokenRequest = new Dictionary<string, string>
        {
            { "code",          code          },
            { "client_id",     _clientId     },
            { "client_secret", _clientSecret },
            { "redirect_uri",  _redirectUri  },
            { "grant_type",    "authorization_code" }
        };

        HttpResponseMessage response;

        try
        {
            response = await _httpClient.PostAsync(
                "https://oauth2.googleapis.com/token",
                new FormUrlEncodedContent(tokenRequest),
                cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while contacting Google token endpoint.");
            throw new InvalidOperationException("Unable to reach Google authentication service. Please try again.", ex);
        }

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        _logger.LogInformation(
            "Google token endpoint responded with status: {StatusCode}",
            response.StatusCode);

        if (!response.IsSuccessStatusCode)
        {
            // Log full details internally — never expose to the caller
            _logger.LogError(
                "Google token exchange failed. Status: {StatusCode}, Body: {Body}",
                response.StatusCode,
                responseBody);

            throw new UnauthorizedAccessException("Google authentication failed. Please try logging in again.");
        }

        // Parse id_token from response
        using var doc = System.Text.Json.JsonDocument.Parse(responseBody);
        var root = doc.RootElement;

        if (!root.TryGetProperty("id_token", out var idTokenElement))
        {
            _logger.LogError("Google token response did not contain an id_token.");
            throw new UnauthorizedAccessException("Google authentication failed: missing identity token.");
        }

        var idToken = idTokenElement.GetString();

        if (string.IsNullOrWhiteSpace(idToken))
        {
            _logger.LogError("Google token response contained an empty id_token.");
            throw new UnauthorizedAccessException("Google authentication failed: empty identity token.");
        }

        return idToken;
    }

    private async Task<OAuthUserInfo> ValidateAndExtractUserInfoAsync(string idToken, CancellationToken cancellationToken)
    {
        // Respect cancellation before an async operation that doesn't natively accept a token
        cancellationToken.ThrowIfCancellationRequested();

        GoogleJsonWebSignature.Payload payload;

        try
        {
            // Validate signature, expiry, issuer, AND audience (prevents token substitution attacks)
            payload = await GoogleJsonWebSignature.ValidateAsync(
                idToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _clientId }
                });
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogWarning(ex, "Google id_token validation failed.");
            throw new UnauthorizedAccessException("Google identity token is invalid or expired. Please log in again.", ex);
        }

        if (string.IsNullOrWhiteSpace(payload.Email))
        {
            _logger.LogError("Google payload is missing email for Subject: {Subject}", payload.Subject);
            throw new UnauthorizedAccessException("Google account did not provide an email address.");
        }

        return new OAuthUserInfo
        {
            Email = payload.Email,
            ExternalUserId = payload.Subject ?? string.Empty,
            Name = payload.Name ?? string.Empty
        };
    }
}