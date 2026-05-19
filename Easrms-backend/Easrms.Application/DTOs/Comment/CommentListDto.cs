namespace Easrms.Application.DTOs.Comment;

public class CommentListDto
{
    public Guid CommentId { get; set; }
    public string CommentText { get; set; } = string.Empty;
    public string CommentType { get; set; }
    public string CommentByName { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
}

