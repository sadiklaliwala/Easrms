namespace Easrms.Application.DTOs.Chat;

public class ChatIntentDto
{
    // Possible values:
    // get_my_requests | get_request_detail | get_dashboard_summary |
    // get_my_approvals | get_my_tasks | get_request_history | get_request_comments | unknown
    public string Intent { get; set; } = string.Empty;

    public ChatIntentFiltersDto Filters { get; set; } = new();
}

public class ChatIntentFiltersDto
{
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public string? RequestNumber { get; set; }
    public string? RequestId { get; set; }
}
