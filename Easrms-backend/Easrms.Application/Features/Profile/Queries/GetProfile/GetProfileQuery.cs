using Easrms.Application.DTOs.Profile;
using Easrms.Common.Response;
using MediatR;

namespace Easrms.Application.Features.Profile.Queries.GetProfile;

public class GetProfileQuery : IRequest<ApiResponse<ProfileDetailDto>>
{
    public Guid CurrentUserId { get; set; }
}
