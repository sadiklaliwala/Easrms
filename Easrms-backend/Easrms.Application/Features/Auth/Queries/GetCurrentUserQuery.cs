using AutoMapper;
using Easrms.Application.DTOs.Auth;
using Easrms.Application.Interfaces.Repositories;
using MediatR;

namespace Easrms.Application.Features.Auth.Queries;

public sealed class GetCurrentUserQuery : IRequest<CurrentUserDto>
{
    public Guid CurrentUserId { get; init; }
}


/// <summary>
/// 1. GetByIdAsync(currentUserId, trackChanges: false) → 401 if null
/// 2. Map entity → CurrentUserDto → return
/// </summary>
public sealed class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, CurrentUserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetCurrentUserQueryHandler(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<CurrentUserDto> Handle(
        GetCurrentUserQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Fetch user
        var user = await _userRepository.GetByIdAsync(
            request.CurrentUserId,
            trackChanges: false,
            cancellationToken: cancellationToken);

        if (user is null)
            throw new UnauthorizedAccessException("User not found.");

        // 2. Map and return
        return _mapper.Map<CurrentUserDto>(user);
    }
}