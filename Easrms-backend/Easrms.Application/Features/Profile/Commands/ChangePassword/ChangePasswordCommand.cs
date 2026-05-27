using Easrms.Common.Response;
using MediatR;

namespace Easrms.Application.Features.Profile.Commands.ChangePassword;

public class ChangePasswordCommand : IRequest<ApiResponse<string>>
{
    public Guid CurrentUserId { get; set; }
    public string PasswordChangeToken { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}
