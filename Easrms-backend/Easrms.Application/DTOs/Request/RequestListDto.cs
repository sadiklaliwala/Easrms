using Easrms.Common.Enums;

namespace Easrms.Application.DTOs.Request;

public class RequestListDto
{
    public Guid RequestId { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public PriorityEnums Priority { get; set; }
    public RequestStatusEnum Status { get; set; }
    public DateTime CreatedOn { get; set; }


    public string AssigneeName { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public string SLAStatus { get; set; } = string.Empty;
    public bool IsEscalated { get; set; }
    public string? AttachmentUrl { get; set; }
}

