namespace Easrms.Common.Helpers;

public static class RequestNumberHelper
{
    public static string Generate()
    {
        var datePart = DateTime.Now.ToString("yyyyMMdd");
        var randomPart = Random.Shared.Next(1000, 9999);
        return $"REQ-{datePart}-{randomPart}";
    }
}