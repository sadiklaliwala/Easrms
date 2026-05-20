using Easrms.Common.Enums;

namespace Easrms.Application.Helpers;

public static class SlaHelper
{
    public static string Calculate(RequestStatusEnum status, DateTime? dueDate)
    {
        if (status == RequestStatusEnum.Resolved || status == RequestStatusEnum.Closed)
            return "Within SLA";

        if (dueDate == null)
            return "N/A";

        var now = DateTime.UtcNow;

        if (now > dueDate.Value)
            return "Breached";

        if (now > dueDate.Value.AddHours(-2))
            return "Nearing Breach";

        return "Within SLA";
    }
}