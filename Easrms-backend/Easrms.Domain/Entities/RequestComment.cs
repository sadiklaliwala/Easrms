namespace Easrms.Domain.Entities;

public class RequestComment
{
    public Guid CommentId { get; set; }

    public Guid RequestId { get; set; }

    public Guid CommentBy { get; set; }

    public string CommentText { get; set; } = string.Empty;

    public string CommentType { get; set; } = string.Empty;

    public DateTime CreatedOn { get; set; }

    public bool IsDeleted { get; set; } = false;


    // Navigation Properties
    public ServiceRequest ServiceRequest { get; set; } = null!;

    public User CommentByUser { get; set; } = null!;
}

