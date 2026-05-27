using Easrms.Application.DTOs.Profile;
using Easrms.Application.Features.Profile.Commands.ChangePassword;
using Easrms.Application.Features.Profile.Commands.SendProfileOtp;
using Easrms.Application.Features.Profile.Commands.UpdateProfile;
using Easrms.Application.Features.Profile.Commands.VerifyProfileOtp;
using Easrms.Application.Features.Profile.Queries.GetProfile;
using Easrms.Common.Response;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Easrms.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProfileController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken = default)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var query = new GetProfileQuery { CurrentUserId = userId };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<ProfileDetailDto>.SuccessResponse(result.Data, result.Message));
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto, CancellationToken cancellationToken = default)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var command = new UpdateProfileCommand { CurrentUserId = userId, Model = dto };
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp(CancellationToken cancellationToken = default)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var command = new SendProfileOtpCommand { UserId = userId };
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyProfileOtpDto dto, CancellationToken cancellationToken = default)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var command = new VerifyProfileOtpCommand { UserId = userId, OtpCode = dto.OtpCode };
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto, CancellationToken cancellationToken = default)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var command = new ChangePasswordCommand { CurrentUserId = userId, PasswordChangeToken = dto.PasswordChangeToken, NewPassword = dto.NewPassword, ConfirmPassword = dto.ConfirmPassword };
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
