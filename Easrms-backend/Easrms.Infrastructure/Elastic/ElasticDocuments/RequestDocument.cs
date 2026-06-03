using System;

namespace Easrms.Infrastructure.Elastic.ElasticDocuments;

public class RequestDocument
{
    public Guid RequestId { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string AssigneeName { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string RejectionReason { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsSLABreached { get; set; }
    public bool IsEscalated { get; set; }
}
