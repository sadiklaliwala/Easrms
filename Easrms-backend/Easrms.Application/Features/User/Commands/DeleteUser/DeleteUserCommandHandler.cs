using Easrms.Application.Interfaces.Repositories;
using Easrms.Common.Response;
using MediatR;

namespace Easrms.Application.Features.User.Commands.DeleteUser;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, ApiResponse<string>>
{
    private readonly IUserRepository _userRepository;

    public DeleteUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ApiResponse<string>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == request.CurrentUserId)
            return ApiResponse<string>.FailResponse("You cannot delete your own account.", 400);

        var exists = await _userRepository.ExistsAsync(request.UserId, cancellationToken);
        if (!exists)
            return ApiResponse<string>.FailResponse("User not found.", 404);

        var ok = await _userRepository.SoftDeleteUserAsync(request.UserId);
        if (!ok)
            return ApiResponse<string>.FailResponse("Failed to delete user.", 500);

        return ApiResponse<string>.SuccessResponse(null, "User deleted successfully.");
    }
}
