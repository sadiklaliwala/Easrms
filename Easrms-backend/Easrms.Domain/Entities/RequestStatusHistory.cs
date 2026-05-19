using Easrms.Common.Enums;

namespace Easrms.Domain.Entities;

public class RequestStatusHistory
{
    public Guid HistoryId { get; set; }

    public Guid RequestId { get; set; }

    public RequestStatusEnum? OldStatus { get; set; }

    public RequestStatusEnum NewStatus { get; set; }

    public Guid ChangedBy { get; set; }

    public DateTime ChangedOn { get; set; }

    public string? Remarks { get; set; }


    // Navigation Properties
    public ServiceRequest Request { get; set; } = null!;

    public User ChangedByUser { get; set; } = null!;
}

