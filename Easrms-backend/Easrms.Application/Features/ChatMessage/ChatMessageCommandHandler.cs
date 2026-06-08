using AutoMapper;
using Easrms.Application.DTOs.Chat;
using Easrms.Application.DTOs.Comment;
using Easrms.Application.DTOs.Dashboard;
using Easrms.Application.DTOs.Request;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Common.Constants;
using Easrms.Common.Response;
using Easrms.Infrastructure.Services;
using MediatR;
using System.Text.Json;

namespace Easrms.Application.Features.ChatMessage;

public class ChatMessageCommandHandler
    : IRequestHandler<ChatMessageCommand, ApiResponse<ChatMessageResponseDto>>
{
    private readonly IClaudeService _claudeService;
    private readonly IRequestRepository _requestRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly IDashboardRepository _dashboardRepository;
    private readonly IMapper _mapper;

    public ChatMessageCommandHandler(
        IClaudeService claudeService,
        IRequestRepository requestRepository,
        ICommentRepository commentRepository,
        IDashboardRepository dashboardRepository,
        IMapper mapper
        )
    {
        _claudeService = claudeService;
        _requestRepository = requestRepository;
        _commentRepository = commentRepository;
        _dashboardRepository = dashboardRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<ChatMessageResponseDto>> Handle(
        ChatMessageCommand command,
        CancellationToken cancellationToken)
    {
        ChatIntentDto intent;
        try
        {
            // ── Step 1: Ask AI to parse the user's intent ──────────────────────
            var intentJson = await _claudeService.ResolveIntentAsync(
                command.Message,
                command.CurrentUserRole);

            // Safety check for invalid AI responses
            if (string.IsNullOrWhiteSpace(intentJson) ||
                !intentJson.TrimStart().StartsWith("{"))
            {
                intentJson = """
        {
          "intent": "unknown",
          "filters": {
            "status": null,
            "priority": null,
            "requestNumber": null,
            "requestId": null
          }
        }
        """;
            }

            intent = JsonSerializer.Deserialize<ChatIntentDto>(
                intentJson,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })
                ?? new ChatIntentDto
                {
                    Intent = "unknown"
                };

            // Fix AI mistakes: if RequestId is not a GUID, it's probably the RequestNumber
            if (intent.Filters != null &&
                !string.IsNullOrWhiteSpace(intent.Filters.RequestId) &&
                !Guid.TryParse(intent.Filters.RequestId, out _))
            {
                if (string.IsNullOrWhiteSpace(intent.Filters.RequestNumber))
                {
                    intent.Filters.RequestNumber = intent.Filters.RequestId;
                }
                intent.Filters.RequestId = null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);

            return ApiResponse<ChatMessageResponseDto>.SuccessResponse(
                new ChatMessageResponseDto
                {
                    Reply = "I am currently experiencing technical difficulties processing your request. Please try again later.",
                    Intent = "error"
                },
                "Failed to reach AI service."
            );
        }

        // ── Step 2: Route to the correct repository based on intent ────────────
        string dataContext = string.Empty;

        switch (intent.Intent)
        {
            // Employee — show own requests (optionally filtered by status/priority)
            case "get_my_requests":
                {
                    var queryParams = new RequestQueryParams
                    {
                        EmployeeId = command.CurrentUserId,
                        Status = intent.Filters.Status,
                        Priority = intent.Filters.Priority,
                        PageNumber = 1,
                        PageSize = 5   // keep bot replies concise
                    };
                    var result = await _requestRepository.GetPagedRequestsAsync(queryParams);
                    dataContext = BuildRequestListContext(result);
                    break;
                }

            // Manager — requests pending their approval
            case "get_my_approvals":
                {
                    var queryParams = new RequestQueryParams
                    {
                        Status = StatusConstants.PendingApproval,
                        ManagerId = command.CurrentUserId,
                        PageNumber = 1,
                        PageSize = 5
                    };
                    var result = await _requestRepository.GetPagedRequestsAsync(queryParams);
                    dataContext = BuildRequestListContext(result);
                    break;
                }

            // Support User — requests assigned to them
            case "get_my_tasks":
                {
                    var queryParams = new RequestQueryParams
                    {
                        AssignedTo = command.CurrentUserId,
                        Status = intent.Filters.Status,
                        PageNumber = 1,
                        PageSize = 5
                    };
                    var result = await _requestRepository.GetPagedRequestsAsync(queryParams);
                    dataContext = BuildRequestListContext(result);
                    break;
                }

            // Any role — detail of a specific request by RequestNumber or RequestId
            case "get_request_detail":
                {
                    if (!string.IsNullOrWhiteSpace(intent.Filters.RequestId)
                        && Guid.TryParse(intent.Filters.RequestId, out var requestId))
                    {
                        var detail = await _requestRepository.GetRequestByIdAsync(requestId);
                        var mappedDetail = _mapper.Map<RequestDetailDto>(detail);
                        dataContext = mappedDetail is null
                            ? "No request found with that ID."
                            : BuildRequestDetailContext(mappedDetail);
                    }
                    else if (!string.IsNullOrWhiteSpace(intent.Filters.RequestNumber))
                    {
                        var detail = await _requestRepository
                            .GetRequestByNumberAsync(intent.Filters.RequestNumber);
                        var mappedDetail = _mapper.Map<RequestDetailDto>(detail);
                        dataContext = mappedDetail is null
                            ? $"No request found with number {intent.Filters.RequestNumber}."
                            : BuildRequestDetailContext(mappedDetail);
                    }
                    else
                    {
                        dataContext = "Please provide a request number or ID.";
                    }
                    break;
                }

            // Admin / Manager — dashboard summary
            case "get_dashboard_summary":
                {
                    var queryParams = new DashboardQueryParams();

                    if (command.CurrentUserRole == RoleConstants.Manager)
                        queryParams.ManagerId = command.CurrentUserId;
                    else if (command.CurrentUserRole == RoleConstants.Employee)
                        queryParams.EmployeeId = command.CurrentUserId;
                    else if (command.CurrentUserRole == RoleConstants.SupportUser)
                        queryParams.AssignedToUserId = command.CurrentUserId;
                    // Admin → all nulls (global scope)

                    var summary = await _dashboardRepository.GetSummaryAsync(queryParams);
                    dataContext = BuildDashboardContext(summary);
                    break;
                }

            // Status history for a specific request
            case "get_request_history":
                {
                    Guid? targetId = null;
                    if (!string.IsNullOrWhiteSpace(intent.Filters.RequestId) && Guid.TryParse(intent.Filters.RequestId, out var parsedId))
                        targetId = parsedId;
                    else if (!string.IsNullOrWhiteSpace(intent.Filters.RequestNumber))
                    {
                        var req = await _requestRepository.GetRequestByNumberAsync(intent.Filters.RequestNumber);
                        if (req != null) targetId = req.RequestId;
                    }

                    if (targetId.HasValue)
                    {
                        var exists = await _requestRepository.ExistsAsync(targetId.Value);
                        if (!exists)
                        {
                            dataContext = "No request found with that ID or Number.";
                            break;
                        }
                        var history = await _commentRepository
                            .GetStatusHistoryByRequestIdAsync(targetId.Value);
                        dataContext = BuildHistoryContext(history);
                    }
                    else
                    {
                        dataContext = "Please provide a valid request number (e.g., REQ-0001) to fetch its history.";
                    }
                    break;
                }

            // Comments for a specific request
            case "get_request_comments":
                {
                    Guid? targetId = null;
                    if (!string.IsNullOrWhiteSpace(intent.Filters.RequestId) && Guid.TryParse(intent.Filters.RequestId, out var parsedId))
                        targetId = parsedId;
                    else if (!string.IsNullOrWhiteSpace(intent.Filters.RequestNumber))
                    {
                        var req = await _requestRepository.GetRequestByNumberAsync(intent.Filters.RequestNumber);
                        if (req != null) targetId = req.RequestId;
                    }

                    if (targetId.HasValue)
                    {
                        var exists = await _requestRepository.ExistsAsync(targetId.Value);
                        if (!exists)
                        {
                            dataContext = "No request found with that ID or Number.";
                            break;
                        }
                        var comments = await _commentRepository
                            .GetCommentsByRequestIdAsync(targetId.Value);
                        dataContext = BuildCommentsContext(comments);
                    }
                    else
                    {
                        dataContext = "Please provide a valid request number (e.g., REQ-0001) to fetch its comments.";
                    }
                }
                break;
        

            default:
                dataContext = string.Empty;
                break;
        }

        // ── Step 3: Ask Claude to format a friendly reply using the fetched data ─
        var friendlyReply = await _claudeService.FormatReplyAsync(
            command.Message,
            intent.Intent,
            dataContext,
            command.CurrentUserName,
            command.CurrentUserRole);

        return ApiResponse<ChatMessageResponseDto>.SuccessResponse(
            new ChatMessageResponseDto
            {
                Reply = friendlyReply,
                Intent = intent.Intent
            },
            "Chat response generated.");
    }

    // ── Private context builders ──────────────────────────────────────────────

    private static string BuildRequestListContext(RequestListWithPaginationDto result)
    {
        if (result.Items is null || result.Items.Count == 0)
            return "No requests found matching your criteria.";

        var lines = result.Items.Select(r =>
            $"- [{r.RequestNumber}] {r.Title} | Status: {r.Status} | Priority: {r.Priority} | Category: {r.CategoryName} | Assignee: {r.AssigneeName ?? "Unassigned"}");

        return $"Total: {result.Pagination.TotalCount} request(s). Showing first {result.Items.Count}:\n"
               + string.Join("\n", lines);
    }

    private static string BuildRequestDetailContext(RequestDetailDto r)
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

    private static string BuildDashboardContext(DashboardSummaryDto s)
    {
        return $"Total: {s.TotalRequests} | Open: {s.OpenCount} | Pending Approval: {s.PendingApprovalCount} | "
             + $"Approved: {s.ApprovedCount} | Rejected: {s.RejectedCount} | Assigned: {s.AssignedCount} | "
             + $"In Progress: {s.InProgressCount} | Resolved: {s.ResolvedCount} | Closed: {s.ClosedCount}";
    }

    private static string BuildHistoryContext(
        IReadOnlyList<StatusHistoryDto> history)
    {
        if (!history.Any()) return "No history found for this request.";

        var lines = history.Select(h =>
            $"- {h.ChangedOn:dd MMM yyyy HH:mm} | {h.OldStatus} → {h.NewStatus} | By: {h.ChangedByName} | Note: {h.Remarks ?? "-"}");

        return string.Join("\n", lines);
    }

    private static string BuildCommentsContext(
        IReadOnlyList<CommentListDto> comments)
    {
        if (!comments.Any()) return "No comments found for this request.";

        var lines = comments.Select(c =>
            $"- [{c.CreatedOn:dd MMM yyyy}] {c.CommentByName}: {c.CommentText}");

        return string.Join("\n", lines);
    }
}
