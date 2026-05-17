using AutoMapper;
using Easrms.Application.DTOs.User;
using Easrms.Application.Interfaces.Repositories;
using MediatR;

namespace Easrms.Application.Features.User.Queries;

public record class GetUserByIdQuery(Guid UserId) :IRequest<UserDetailDto>
{
    

}

public class GetUserByIdQueryHandler(IUserRepository userRepository , IMapper mapper) : IRequestHandler<GetUserByIdQuery, UserDetailDto>
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<UserDetailDto> Handle(
        GetUserByIdQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, trackChanges: false, cancellationToken);

        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {request.UserId} not found.");

        }
        return _mapper.Map<UserDetailDto>(user);
        
    }
}

