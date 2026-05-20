// Easrms.Infrastructure/Services/JwtService.cs

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Easrms.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Easrms.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Easrms.Application.Settings;

namespace Easrms.Infrastructure.Services;

/// <summary>
/// Concrete implementation of <see cref="IJwtService"/>.
/// Handles JWT access token generation, refresh token generation,
/// expired token principal extraction, HttpOnly cookie management,
/// and current-user claim extraction from HttpContext.
///
/// Registered as Scoped in ServiceExtensions.cs:
///   services.AddScoped&lt;IJwtService, JwtService&gt;();
///
/// Depends on:
///   - IOptions&lt;JwtSettings&gt; for configuration
///   - IHttpContextAccessor for cookie and claims access
/// </summary>
public sealed class JwtService : IJwtService
{
    // ─────────────────────────────────────────────────────────────────────────
    // CLAIM TYPE CONSTANTS
    // Centralized here to avoid magic strings scattered across the codebase.
    // ─────────────────────────────────────────────────────────────────────────
    private const string ClaimUserId = ClaimTypes.NameIdentifier;
    private const string ClaimEmail = ClaimTypes.Email;
    private const string ClaimRole = ClaimTypes.Role;
    private const string ClaimFullName = "fullName";
    private const string ClaimManagerId = "managerId";

    private readonly JwtSettings _jwtSettings;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public JwtService(
        IOptions<JwtSettings> jwtSettings,
        IHttpContextAccessor httpContextAccessor)
    {
        _jwtSettings = jwtSettings.Value;
        _httpContextAccessor = httpContextAccessor;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GENERATE ACCESS TOKEN
    // ─────────────────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public string GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier,   user.UserId.ToString()),
            new(ClaimTypes.Email,    user.Email),
            new(ClaimTypes.Role,     user.Role.RoleName),
            new(ClaimFullName, user.FullName),
        };

        // ManagerId is optional — only employees have a manager.
        // Embedding it in the token avoids a DB round-trip in the approval handler.
        if (user.ManagerId.HasValue)
            claims.Add(new Claim(ClaimManagerId, user.ManagerId.Value.ToString()));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
            SigningCredentials = credentials,
            NotBefore = DateTime.UtcNow,
            IssuedAt = DateTime.UtcNow,
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(tokenDescriptor);
        return handler.WriteToken(token);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GENERATE REFRESH TOKEN
    // ─────────────────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public string GenerateRefreshToken()
    {
        // 64 bytes of cryptographic randomness → URL-safe Base64 string.
        // Not derived from any user data — purely opaque random value.
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }



    // ─────────────────────────────────────────────────────────────────────────
    // COOKIE MANAGEMENT
    // ─────────────────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public void SetTokenCookie(string token)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,                    // Not accessible via JavaScript
            Secure = _jwtSettings.CookieSecure,                    // HTTPS only
            SameSite = SameSiteMode.None,     // No cross-site requests
            Expires = DateTimeOffset.UtcNow
                            .AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
            Path = "/",                     // Available to all API routes
        };

        _httpContextAccessor.HttpContext!.Response.Cookies.Append(
            _jwtSettings.CookieName,
            token,
            cookieOptions);
    }

    /// <inheritdoc/>
    public void ClearTokenCookie()
    {
        // Overwrite the cookie with an empty value and an already-expired date.
        // This is the correct way to delete a cookie — Delete() alone is unreliable
        // across some browsers when the original cookie had explicit options set.
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = _jwtSettings.CookieSecure,
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddDays(-1), // expired = deleted
            Path = "/",
        };

        _httpContextAccessor.HttpContext!.Response.Cookies.Append(
            _jwtSettings.CookieName,
            string.Empty,
            cookieOptions);
    }

    public void ClearRefreshTokenCookie()
    {
        // Overwrite the cookie with an empty value and an already-expired date.
        // This is the correct way to delete a cookie — Delete() alone is unreliable
        // across some browsers when the original cookie had explicit options set.
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = _jwtSettings.CookieSecure,
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddDays(-1), // expired = deleted
            Path = "/",
        };

        _httpContextAccessor.HttpContext!.Response.Cookies.Append(
            _jwtSettings.CookieNameRefresh,
            string.Empty,
            cookieOptions);
    }

    /// <inheritdoc/>
    public void SetRefreshTokenCookie(string token)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = _jwtSettings.CookieSecure,
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
            Path = "/",
        };

        _httpContextAccessor.HttpContext!.Response.Cookies.Append(
            _jwtSettings.CookieNameRefresh,
            token,
            cookieOptions);
    }

    /// <inheritdoc/>
    public string? GetRefreshTokenFromCookie()
    {
        return _httpContextAccessor.HttpContext?.Request.Cookies[_jwtSettings.CookieNameRefresh];
    }

}