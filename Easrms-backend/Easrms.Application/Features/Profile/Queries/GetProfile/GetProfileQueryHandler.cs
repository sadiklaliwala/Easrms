using AutoMapper;
using Easrms.Application.DTOs.Profile;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Common.Response;
using MediatR;

namespace Easrms.Application.Features.Profile.Queries.GetProfile;

public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, ApiResponse<ProfileDetailDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetProfileQueryHandler(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<ProfileDetailDto>> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.CurrentUserId);
        if (user == null)
            return ApiResponse<ProfileDetailDto>.FailResponse("User not found", 400);

        var dto = _mapper.Map<ProfileDetailDto>(user);
        return ApiResponse<ProfileDetailDto>.SuccessResponse(dto, "Profile retrieved successfully.");
    }
}
