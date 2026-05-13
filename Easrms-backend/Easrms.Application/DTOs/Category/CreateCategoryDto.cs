namespace Easrms.Application.DTOs.Category;

public class CreateCategoryDto
{
    public string CategoryName { get; set; } = string.Empty;
    public bool IsApprovalRequired { get; set; }
}

