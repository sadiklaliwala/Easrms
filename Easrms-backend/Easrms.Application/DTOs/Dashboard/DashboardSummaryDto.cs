namespace Easrms.Application.DTOs.Dashboard;

public class DashboardSummaryDto
{
    public int TotalRequests { get; set; }
    public int OpenCount { get; set; }
    public int PendingApprovalCount { get; set; }
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }
    public int AssignedCount { get; set; }
    public int InProgressCount { get; set; }
    public int ResolvedCount { get; set; }
    public int ClosedCount { get; set; }
    public List<PriorityCountDto> ByPriority { get; set; } = new List<PriorityCountDto>();
    public List<CategoryCountDto> ByCategory { get; set; } = new List<CategoryCountDto>();
}

