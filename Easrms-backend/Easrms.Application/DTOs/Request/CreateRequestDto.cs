using Easrms.Common.Enums;

namespace Easrms.Application.DTOs.Request;

public class CreateRequestDto
{
    public Guid CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PriorityEnums Priority { get; set; } = PriorityEnums.Low;
    public string? AttachmentUrl { get; set; }
}

