using Easrms.Application.Features.Comment.Commands;
using Easrms.Application.Features.Comment.Queries;
using Easrms.Application.Features.History.Queries;
using Easrms.Application.DTOs.Comment;
using Easrms.Common.Response;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Easrms.API.Controllers;

[ApiController]
[Route("api/requests/{requestId:guid}")]
[Authorize]
public class CommentController : ControllerBase
{
    private readonly IMediator _mediator;

    public CommentController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // POST /api/requests/{requestId}/comments
    [HttpPost("comments")]
    public async Task<IActionResult> AddComment(
        Guid requestId,
        [FromBody] AddCommentDto dto,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var command = new AddCommentCommand
        {
            RequestId = requestId,
            CommentText = dto.CommentText,
            CommentType = dto.CommentType,
            CommentBy = currentUserId
        };

        var commentId = await _mediator.Send(command, cancellationToken);

        return StatusCode(201, new ApiResponse<object>
        {
            Success = true,
            StatusCode = 201,
            Message = "Comment added successfully.",
            Data = new { CommentId = commentId },
            Errors = null
        });
    }

    // GET /api/requests/{requestId}/comments
    [HttpGet("comments")]
    public async Task<IActionResult> GetComments(
        Guid requestId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCommentsQuery { RequestId = requestId };

        var result = await _mediator.Send(query, cancellationToken);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            StatusCode = 200,
            Message = "Comments retrieved successfully.",
            Data = result,
            Errors = null
        });
    }

    // GET /api/requests/{requestId}/history
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(
        Guid requestId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetStatusHistoryQuery { RequestId = requestId };

        var result = await _mediator.Send(query, cancellationToken);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            StatusCode = 200,
            Message = "Status history retrieved successfully.",
            Data = result,
            Errors = null
        });
    }
}