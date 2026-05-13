namespace Easrms.Application.DTOs.Category;

public class UpdateCategoryDto
{
    public string CategoryName { get; set; } = string.Empty;
    public bool IsApprovalRequired { get; set; }
}

