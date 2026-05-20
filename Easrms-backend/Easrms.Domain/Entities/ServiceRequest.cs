using Easrms.Common.Enums;

namespace Easrms.Domain.Entities;

public class ServiceRequest
{
    public Guid RequestId { get; set; }

    public string RequestNumber { get; set; } = string.Empty;

    public Guid EmployeeId { get; set; }

    public Guid CategoryId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public PriorityEnums Priority { get; set; }

    public RequestStatusEnum Status { get; set; }

    public Guid? AssignedTo { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public DateTime? ResolvedOn { get; set; }

    public DateTime? ClosedOn { get; set; }

    public Guid? ClosedBy { get; set; }

    public string? RejectionReason { get; set; }

    // CR-001 added ──────────────────────────
    public DateTime? DueDate { get; set; }
    public bool IsEscalated { get; set; }
    public DateTime? EscalatedOn { get; set; }
    public Guid? EscalatedBy { get; set; }
    public string? EscalationReason { get; set; }
    // ────────────────────────────────────────


    // Navigation Properties
    public User Employee { get; set; } = null!;

    public RequestCategory Category { get; set; } = null!;

    public User? AssignedUser { get; set; }

    public User? ClosedByUser { get; set; }

    public User? Escalator { get; set; }  // CR-001 added

    public ICollection<RequestComment> Comments { get; set; } = new List<RequestComment>();
    public ICollection<RequestEscalationHistory> EscalationHistories { get; set; } = new List<RequestEscalationHistory>(); // CR-001 added

    public ICollection<RequestStatusHistory> StatusHistories { get; set; } = new List<RequestStatusHistory>();
}

