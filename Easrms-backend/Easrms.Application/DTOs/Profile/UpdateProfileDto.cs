namespace Easrms.Application.DTOs.Profile;

public class UpdateProfileDto
{
    public string FullName { get; set; } = string.Empty;
    public string? ProfilePhotoUrl { get; set; }
}
