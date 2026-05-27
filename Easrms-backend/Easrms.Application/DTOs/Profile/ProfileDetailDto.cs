namespace Easrms.Application.DTOs.Profile;

public class ProfileDetailDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string? ManagerName { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? LastLoginOn { get; set; }
}
