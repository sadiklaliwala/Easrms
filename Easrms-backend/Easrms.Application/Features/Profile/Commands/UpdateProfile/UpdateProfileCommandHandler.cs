using AutoMapper;
using Easrms.Application.DTOs.Profile;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Common.Response;
using MediatR;

namespace Easrms.Application.Features.Profile.Commands.UpdateProfile;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, ApiResponse<ProfileDetailDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UpdateProfileCommandHandler(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<ProfileDetailDto>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.CurrentUserId, trackChanges: true);
        if (user == null)
            return ApiResponse<ProfileDetailDto>.FailResponse("User not found", 400);

        user.FullName = request.Model.FullName?.Trim();
        if (!string.IsNullOrWhiteSpace(request.Model.ProfilePhotoUrl))
            user.ProfilePhotoUrl = request.Model.ProfilePhotoUrl;

        await _userRepository.UpdateAsync(user);

        var dto = _mapper.Map<ProfileDetailDto>(user);
        return ApiResponse<ProfileDetailDto>.SuccessResponse(dto, "Profile updated successfully.");
    }
}
