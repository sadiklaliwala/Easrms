namespace Easrms.Application.DTOs.Request;

public class UpdateStatusDto
{
    public int NewStatus { get; set; }
    public string Remarks { get; set; } = string.Empty;
}

