// Easrms.Application/Interfaces/IJwtService.cs

using System.Security.Claims;
using Easrms.Domain.Entities;

namespace Easrms.Application.Interfaces;

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
    /// Validates the signature and structure of a JWT access token and extracts its claims.
    /// IMPORTANT: This method deliberately does NOT validate token expiry —
    /// it is used specifically during the refresh-token flow where the access token
    /// is expected to be expired. Lifetime validation is skipped intentionally.
    /// Returns null if the token signature is invalid or the token is malformed.
    /// </summary>
    /// <param name="accessToken">The expired or active JWT access token string.</param>
    /// <returns>
    /// A <see cref="ClaimsPrincipal"/> containing the token's claims if valid;
    /// null if the token cannot be validated.
    /// </returns>
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string accessToken);

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
    /// Extracts the authenticated user's ID from the current HTTP request's JWT claims.
    /// Reads the NameIdentifier claim embedded during token generation.
    /// Throws <see cref="UnauthorizedAccessException"/> if the claim is missing or unparseable —
    /// this should never happen on a correctly authenticated request.
    /// </summary>
    /// <returns>The current user's <see cref="Guid"/> UserId.</returns>
    Guid GetCurrentUserId();

    /// <summary>
    /// Extracts the authenticated user's role name from the current HTTP request's JWT claims.
    /// Reads the Role claim embedded during token generation.
    /// Returns null if the role claim is not present (should not happen on authenticated requests).
    /// </summary>
    /// <returns>The current user's role name string, or null if claim is absent.</returns>
    string? GetCurrentUserRole();

    /// <summary>
    /// Extracts the authenticated user's ManagerId from the current HTTP request's JWT claims.
    /// Returns null if the user has no manager (Admin, top-level Manager).
    /// Used by the approval handler to verify the caller is the employee's direct manager.
    /// </summary>
    /// <returns>The current user's ManagerId as <see cref="Guid"/>, or null if not set.</returns>
    Guid? GetCurrentUserManagerId();
}