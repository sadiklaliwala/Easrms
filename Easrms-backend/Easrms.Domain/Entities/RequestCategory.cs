namespace Easrms.Domain.Entities;

public class RequestCategory
{
    public Guid CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public bool IsApprovalRequired { get; set; } = false;

    public bool IsActive { get; set; } = true;

    public int SLAHours { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }


    // Navigation Properties
    public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
}

