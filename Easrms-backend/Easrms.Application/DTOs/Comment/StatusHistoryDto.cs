namespace Easrms.Application.DTOs.Comment;

public class StatusHistoryDto
{
    public Guid HistoryId { get; set; }
    public int? OldStatus { get; set; }
    public int NewStatus { get; set; }
    public string ChangedByName { get; set; } = string.Empty;
    public DateTime ChangedOn { get; set; }
    public string? Remarks { get; set; }
}

