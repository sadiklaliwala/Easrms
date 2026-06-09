using Easrms.Application.DTOs.Comment;
using Easrms.Application.DTOs.Dashboard;
using Easrms.Application.DTOs.Request;
using Easrms.Common.Enums;

namespace Easrms.Application.Features.ChatMessage.Intents;

public static class ContextBuilder
{
    public static string BuildRequestListContext(RequestListWithPaginationDto result)
    {
        if (result.Items is null || result.Items.Count == 0)
            return "No requests found matching your criteria.";

        var lines = result.Items.Select(r =>
            $"- [{r.RequestNumber}] {r.Title} | Status: {r.Status} | Priority: {r.Priority} | Category: {r.CategoryName} | Assignee: {r.AssigneeName ?? "Unassigned"}");

        return $"Total: {result.Pagination.TotalCount} request(s). Showing first {result.Items.Count}:\n"
               + string.Join("\n", lines);
    }

    public static string BuildRequestDetailContext(RequestDetailDto r)
    {
        return $"Request Number : {r.RequestNumber}\n"
             + $"Title          : {r.Title}\n"
             + $"Description    : {r.Description}\n"
             + $"Category       : {r.CategoryName}\n"
             + $"Priority       : {r.Priority}\n"
             + $"Status         : {r.Status}\n"
             + $"Raised By      : {r.EmployeeName}\n"
             + $"Assigned To    : {r.AssigneeName ?? "Unassigned"}\n"
             + $"Created On     : {r.CreatedOn:dd MMM yyyy}\n"
             + $"Resolved On    : {(r.ResolvedOn.HasValue ? r.ResolvedOn.Value.ToString("dd MMM yyyy") : "Not yet")}\n"
             + $"Rejection Note : {(string.IsNullOrWhiteSpace(r.RejectionReason) ? "N/A" : r.RejectionReason)}";
    }

    public static string BuildDashboardContext(DashboardSummaryDto s)
    {
        return $"Total: {s.TotalRequests} | Open: {s.OpenCount} | Pending Approval: {s.PendingApprovalCount} | "
             + $"Approved: {s.ApprovedCount} | Rejected: {s.RejectedCount} | Assigned: {s.AssignedCount} | "
             + $"In Progress: {s.InProgressCount} | Resolved: {s.ResolvedCount} | Closed: {s.ClosedCount}";
    }

    public static string BuildHistoryContext(IReadOnlyList<StatusHistoryDto> history)
    {
        if (!history.Any()) return "No history found for this request.";

        var lines = history.Select(h =>
            $"- {h.ChangedOn:dd MMM yyyy HH:mm} | {h.OldStatus} → {h.NewStatus} | By: {h.ChangedByName} | Note: {h.Remarks ?? "-"}");

        return string.Join("\n", lines);
    }

    public static string BuildCommentsContext(IReadOnlyList<CommentListDto> comments)
    {
        if (!comments.Any()) return "No comments found for this request.";

        var lines = comments.Select(c =>
            $"- {c.CreatedOn:dd MMM yyyy HH:mm} | {c.CommentByName} | {(c.CommentType == CommentTypeEnum.Feedback ? "[INTERNAL] " : "")}{c.CommentText}");

        return string.Join("\n", lines);
    }
}
