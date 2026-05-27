namespace Easrms.Application.DTOs.Profile;

public class ChangePasswordDto
{
    public string PasswordChangeToken { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}
