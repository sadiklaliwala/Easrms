namespace Easrms.Application.DTOs.Dashboard;

public class AssignedUserCountDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int Count { get; set; }
}