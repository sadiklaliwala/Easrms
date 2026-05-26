using Easrms.Application.DTOs.Auth;
using Easrms.Application.Features.Auth.Commands;
using Easrms.Application.Features.Auth.Commands.LinkProvider;
using Easrms.Application.Features.Auth.Commands.OAuthLogin;
using Easrms.Application.Features.Auth.Commands.UnlinkProvider;
using Easrms.Application.Features.Auth.Queries;
using Easrms.Application.Features.Auth.Queries.GetLinkedProviders;
using Easrms.Common.Response;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Easrms.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // POST /api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new LoginCommand
        {
            Email = dto.Email,
            Password = dto.Password
        };

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            StatusCode = 200,
            Message = "Login successful.",
            Data = result,
            Errors = null
        });
    }

    // POST /api/auth/logout
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(
        CancellationToken cancellationToken = default)
    {
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var command = new LogoutCommand { CurrentUserId = currentUserId };

        await _mediator.Send(command, cancellationToken);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            StatusCode = 200,
            Message = "Logout successful.",
            Data = null,
            Errors = null
        });
    }

    // GET /api/auth/me
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(
        CancellationToken cancellationToken = default)
    {
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var query = new GetCurrentUserQuery { CurrentUserId = currentUserId };
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            StatusCode = 200,
            Message = "Current user retrieved successfully.",
            Data = result,
            Errors = null
        });
    }

    // POST /api/auth/refresh-token
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken(CancellationToken ct)
    {
        var refreshToken = Request.Cookies["CookieNameRefresh"]; // pick a name
        if (string.IsNullOrWhiteSpace(refreshToken))
            return Unauthorized(ApiResponse<object>.FailResponse("Missing refresh token.", 401));

        var result = await _mediator.Send(new RefreshTokenCommand { RefreshToken = refreshToken }, ct);

        return Ok(ApiResponse<object>.SuccessResponse(result, "Token refreshed successfully."));
    }

    // POST /api/auth/revoke-token
    [HttpPost("revoke-token")]
    [Authorize]
    public async Task<IActionResult> RevokeToken(CancellationToken cancellationToken = default)
    {
        var refreshToken = Request.Cookies["easrms_access_token_refresh"]; // pick a name
        if (string.IsNullOrWhiteSpace(refreshToken))
            return Unauthorized(ApiResponse<object>.FailResponse("Missing refresh token.", 401));

        var command = new RevokeTokenCommand { RefreshToken = refreshToken };

        await _mediator.Send(command, cancellationToken);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            StatusCode = 200,
            Message = "Token revoked successfully.",
            Data = null,
            Errors = null
        });
    }

    // POST /api/auth/oauth-login
    [HttpPost("oauth-login")]
    public async Task<IActionResult> OAuthLogin(
        [FromBody] OAuthLoginDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new OAuthLoginCommand(dto);
        var result = await _mediator.Send(command, cancellationToken);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            StatusCode = 200,
            Message = "Login successful.",
            Data = result,
            Errors = null
        });
    }

    // POST /api/auth/link-provider
    [HttpPost("link-provider")]
    [Authorize]
    public async Task<IActionResult> LinkProvider(
        [FromBody] LinkProviderDto dto,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var command = new LinkProviderCommand(dto, currentUserId);
        await _mediator.Send(command, cancellationToken);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            StatusCode = 200,
            Message = "Provider linked successfully.",
            Data = null,
            Errors = null
        });
    }

    // DELETE /api/auth/unlink-provider
    [HttpDelete("unlink-provider")]
    [Authorize]
    public async Task<IActionResult> UnlinkProvider(
        [FromBody] UnlinkProviderDto dto,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var command = new UnlinkProviderCommand(dto.ProviderId, currentUserId);
        await _mediator.Send(command, cancellationToken);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            StatusCode = 200,
            Message = "Provider unlinked successfully.",
            Data = null,
            Errors = null
        });
    }

    // GET /api/auth/linked-providers
    [HttpGet("linked-providers")]
    [Authorize]
    public async Task<IActionResult> GetLinkedProviders(CancellationToken cancellationToken = default)
    {
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var query = new GetLinkedProvidersQuery(currentUserId);
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            StatusCode = 200,
            Message = "Linked providers retrieved.",
            Data = result,
            Errors = null
        });
    }
}