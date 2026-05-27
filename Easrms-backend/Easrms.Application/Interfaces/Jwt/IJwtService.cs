// Easrms.Application/Interfaces/IJwtService.cs

using System.Security.Claims;
using Easrms.Domain.Entities;

namespace Easrms.Application.Interfaces.Jwt;

/// <summary>
/// Service contract for all JWT access token and refresh token operations.
///
/// Responsibilities:
///   - Generate signed JWT access tokens from a User entity
///   - Generate cryptographically secure refresh tokens
///   - Validate and extract claims from an expired (or active) access token
///   - Set and clear JWT as an HttpOnly cookie on the HTTP response
///   - Extract the current user's ID and role from HttpContext claims
///
/// This service is registered as Scoped in DI — one instance per HTTP request.
/// It does NOT interact with the database — token persistence (storing RefreshToken
/// on the User entity) is handled by the command handlers via IUserRepository.
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generates a signed JWT access token for the given user.
    /// Claims included: UserId (NameIdentifier), Email, RoleName, FullName, ManagerId (if set).
    /// Token is signed with HMAC-SHA256 using the secret from JwtSettings.
    /// Expiry is driven by JwtSettings.AccessTokenExpiryMinutes.
    /// </summary>
    /// <param name="user">
    /// The fully loaded User entity — must have Role navigation populated
    /// so RoleName can be embedded as a claim.
    /// </param>
    /// <returns>A signed JWT access token string.</returns>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Generates a cryptographically secure, URL-safe refresh token.
    /// Uses RandomNumberGenerator (64 bytes → Base64) — not derived from user data.
    /// The caller (command handler) is responsible for persisting this value
    /// on the User entity along with RefreshTokenExpiryOn before calling SaveChangesAsync.
    /// </summary>
    /// <returns>A Base64-encoded random refresh token string.</returns>
    string GenerateRefreshToken();

    /// <summary>
    /// Generates a short-lived JWT used for password reset/change flows.
    /// Contains claims: sub (user id) and purpose.
    /// Lifetime: 15 minutes.
    /// </summary>
    /// <param name="userId">The user's id</param>
    /// <param name="purpose">Either "password-reset" or "password-change"</param>
    /// <returns>Signed JWT string.</returns>
    string GeneratePasswordToken(Guid userId, string purpose);

    /// <summary>
    /// Validates a password token and ensures the purpose claim matches expectedPurpose.
    /// Returns the ClaimsPrincipal if valid; null otherwise.
    /// </summary>
    ClaimsPrincipal? ValidatePasswordToken(string token, string expectedPurpose);
    
    /// <summary>
    /// Writes the JWT access token into an HttpOnly, Secure, SameSite=Strict cookie
    /// on the HTTP response. The cookie name is driven by JwtSettings.CookieName.
    /// Cookie expiry matches the token expiry (JwtSettings.AccessTokenExpiryMinutes).
    /// HttpOnly = true ensures the token is inaccessible from JavaScript.
    /// </summary>
    /// <param name="token">The signed JWT access token string to store in the cookie.</param>
    void SetTokenCookie(string token);

    /// <summary>
    /// Clears the JWT cookie from the HTTP response by overwriting it with
    /// an expired empty cookie. Called during logout and token revocation.
    /// Uses the same cookie name as SetTokenCookie for consistency.
    /// </summary>
    void ClearTokenCookie();

    /// <summary>
    /// Writes the refresh token into an HttpOnly, Secure, SameSite=Strict cookie
    /// on the HTTP response. The cookie name is driven by JwtSettings.RefreshCookieName.
    /// Cookie expiry matches the refresh token expiry (persisted on the User entity).
    /// HttpOnly = true ensures the token is inaccessible from JavaScript.
    /// </summary>
    /// <param name="token">The refresh token string to store in the cookie.</param>
    void SetRefreshTokenCookie(string token);

    /// <summary>
    /// Reads the refresh token from the HTTP request's cookies.
    /// Uses the cookie name configured in JwtSettings.RefreshCookieName.
    /// Returns null if the cookie is not present or cannot be parsed.
    /// </summary>
    /// <returns>
    /// The refresh token string if the cookie is present and valid;
    /// null if the cookie is not set or invalid.
    /// </returns>
    string? GetRefreshTokenFromCookie();
    void ClearRefreshTokenCookie();
}