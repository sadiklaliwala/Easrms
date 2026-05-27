using Easrms.Application.DTOs.Profile;
using Easrms.Common.Response;
using MediatR;

namespace Easrms.Application.Features.Profile.Commands.UpdateProfile;

public class UpdateProfileCommand : IRequest<ApiResponse<ProfileDetailDto>>
{
    public Guid CurrentUserId { get; set; }
    public UpdateProfileDto Model { get; set; } = new UpdateProfileDto();
}
