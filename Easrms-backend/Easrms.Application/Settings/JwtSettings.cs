// Easrms.Common/Settings/JwtSettings.cs


// Easrms.Common/Settings/JwtSettings.cs

using Easrms.Application.Interfaces;

namespace Easrms.Application.Settings;

/// <summary>
/// Strongly-typed configuration model for JWT settings.
/// Bound from the "JwtSettings" section in appsettings.json via IOptions&lt;JwtSettings&gt;.
/// Registered in ServiceExtensions.cs:
///   services.Configure&lt;JwtSettings&gt;(configuration.GetSection("JwtSettings"));
/// </summary>
public sealed class JwtSettings : IJwtSettings
{
    /// <summary>
    /// The secret key used to sign JWT tokens with HMAC-SHA256.
    /// Must be at least 32 characters (256 bits) for HS256.
    /// Store in appsettings.Development.json locally — use environment variable or
    /// Azure Key Vault in production. Never commit the real secret to source control.
    /// appsettings key: JwtSettings:Secret
    /// </summary>
    public string Secret { get; init; } = string.Empty;

    /// <summary>
    /// The token issuer claim embedded in every JWT.
    /// Must match the ValidIssuer in the JWT bearer middleware configuration.
    /// appsettings key: JwtSettings:Issuer
    /// Example value: "Easrms.API"
    /// </summary>
    public string Issuer { get; init; } = string.Empty;

    /// <summary>
    /// The token audience claim embedded in every JWT.
    /// Must match the ValidAudience in the JWT bearer middleware configuration.
    /// appsettings key: JwtSettings:Audience
    /// Example value: "Easrms.Client"
    /// </summary>
    public string Audience { get; init; } = string.Empty;

    /// <summary>
    /// JWT access token lifetime in minutes.
    /// After this period, the token is expired and the refresh-token flow must be used.
    /// appsettings key: JwtSettings:AccessTokenExpiryMinutes
    /// Recommended value for development: 60. For production: 15.
    /// </summary>
    public int AccessTokenExpiryMinutes { get; init; } = 60;

    /// <summary>
    /// Refresh token lifetime in days.
    /// After this period, the user must log in again — no silent re-authentication.
    /// appsettings key: JwtSettings:RefreshTokenExpiryDays
    /// Recommended value: 7.
    /// </summary>
    public int RefreshTokenExpiryDays { get; init; } = 7;

    /// <summary>
    /// The name of the HttpOnly cookie that stores the JWT access token.
    /// Must be consistent across SetTokenCookie and ClearTokenCookie.
    /// appsettings key: JwtSettings:CookieName
    /// Example value: "easrms_access_token"
    /// </summary>
    public string CookieName { get; init; } = string.Empty;
}