using Easrms.Common.Response;
using MediatR;

namespace Easrms.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommand : IRequest<ApiResponse<string>>
{
    public string Email { get; set; } = string.Empty;
    public string PasswordResetToken { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}
