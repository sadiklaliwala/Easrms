using Easrms.Common.Enums;

namespace Easrms.Application.DTOs.Request;

public class UpdateStatusDto
{
    public RequestStatusEnum NewStatus { get; set; }
    public string Remarks { get; set; } = string.Empty;
}

