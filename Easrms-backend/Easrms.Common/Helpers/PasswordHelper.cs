namespace Easrms.Common.Helpers;

public static class PasswordHelper
{
    private const int WorkFactor = 10;

    public static string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public static bool Verify(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}