namespace Easrms.Application.DTOs.Category;

public class CategoryListDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public bool IsApprovalRequired { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedOn { get; set; }
    public int SLAHours { get; set; }
}

