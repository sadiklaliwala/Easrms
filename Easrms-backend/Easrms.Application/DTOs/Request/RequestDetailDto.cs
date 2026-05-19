using Easrms.Common.Enums;

namespace Easrms.Application.DTOs.Request;

public class RequestDetailDto
{
    public Guid RequestId { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public PriorityEnums Priority { get; set; }
    public RequestStatusEnum Status { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public Guid EmployeeId { get; set; }
    public string AssigneeName { get; set; } = string.Empty;
    public Guid? AssignedTo { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public DateTime? ResolvedOn { get; set; }
    public DateTime? ClosedOn { get; set; }
    public string? RejectionReason { get; set; }
}

