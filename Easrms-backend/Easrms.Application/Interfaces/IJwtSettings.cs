// Easrms.Common/Settings/JwtSettings.cs

namespace Easrms.Application.Interfaces
{
    public interface IJwtSettings
    {
        int AccessTokenExpiryMinutes { get; init; }
        string Audience { get; init; }
        string CookieName { get; init; }
        string Issuer { get; init; }
        int RefreshTokenExpiryDays { get; init; }
        string Secret { get; init; }
    }
}