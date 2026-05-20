namespace Easrms.Application.DTOs.Auth;

public class LoginResponseDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public Guid? ManagerId { get; set; }
    
    
}

