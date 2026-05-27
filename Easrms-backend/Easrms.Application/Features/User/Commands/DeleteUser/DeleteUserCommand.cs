using Easrms.Common.Response;
using MediatR;

namespace Easrms.Application.Features.User.Commands.DeleteUser;

public class DeleteUserCommand : IRequest<ApiResponse<string>>
{
    public Guid UserId { get; set; }
    public Guid CurrentUserId { get; set; }
}
