namespace Easrms.Application.DTOs.Comment;

public class AddCommentDto
{
    public string CommentText { get; set; } = string.Empty;
    public int CommentType { get; set; }
}

