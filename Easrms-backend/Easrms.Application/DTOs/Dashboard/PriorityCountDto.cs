using Easrms.Common.Enums;

namespace Easrms.Application.DTOs.Dashboard;

public class PriorityCountDto
{
    public PriorityEnums Priority { get; set; }
    public int Count { get; set; }
}

