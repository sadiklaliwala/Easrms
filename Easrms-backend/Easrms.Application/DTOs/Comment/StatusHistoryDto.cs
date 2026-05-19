using Easrms.Common.Enums;

namespace Easrms.Application.DTOs.Comment;

public class StatusHistoryDto
{
    public Guid HistoryId { get; set; }
    public RequestStatusEnum? OldStatus { get; set; }
    public RequestStatusEnum NewStatus { get; set; }
    public string ChangedByName { get; set; } = string.Empty;
    public DateTime ChangedOn { get; set; }
    public string? Remarks { get; set; }
}

