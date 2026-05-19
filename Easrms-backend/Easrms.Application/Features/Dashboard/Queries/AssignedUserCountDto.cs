namespace Easrms.Application.Features.Dashboard.Queries;

public class AssignedUserCountDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int Count { get; set; }
}