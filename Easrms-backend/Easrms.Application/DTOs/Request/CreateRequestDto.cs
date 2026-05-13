namespace Easrms.Application.DTOs.Request;

public class CreateRequestDto
{
    public Guid CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Priority { get; set; }
}

