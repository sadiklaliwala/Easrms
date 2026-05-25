using Easrms.Common.Enums;

namespace Easrms.Domain.Entities;

public class RequestComment
{
    public Guid CommentId { get; set; }

    public Guid RequestId { get; set; }

    public Guid CommentBy { get; set; }

    public string CommentText { get; set; } = string.Empty;

    // Store the comment type as a strongly-typed enum in the domain model
    public CommentTypeEnum CommentType { get; set; }

    public DateTime CreatedOn { get; set; }

    public bool IsDeleted { get; set; } = false;


    // Navigation Properties
    public ServiceRequest ServiceRequest { get; set; } = null!;

    public User CommentByUser { get; set; } = null!;
}

