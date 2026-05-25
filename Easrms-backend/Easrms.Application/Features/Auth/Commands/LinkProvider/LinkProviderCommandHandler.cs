using Easrms.Application.DTOs.Auth;
using Easrms.Application.Interfaces.OAuth;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Common.Constants;
using Easrms.Domain.Entities;

using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Easrms.Application.Features.Auth.Commands.LinkProvider;

public class LinkProviderCommandHandler : IRequestHandler<LinkProviderCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthProviderRepository _authRepo;
    private readonly IEnumerable<IOAuthService> _oauthServices;

    public LinkProviderCommandHandler(IEnumerable<IOAuthService> oauthServices,IUserRepository userRepository, IAuthProviderRepository authRepo, IServiceProvider sp)
    {
        _userRepository = userRepository;
        _authRepo = authRepo;
        _oauthServices = oauthServices;
    }

    public async Task<bool> Handle(LinkProviderCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.CurrentUserId, trackChanges: false, cancellationToken: cancellationToken);
        if (user == null) throw new KeyNotFoundException("User not found.");

        IOAuthService oauthService = GetServiceForProvider(request.Dto.Provider);
        var userInfo = await oauthService.GetUserInfoAsync(request.Dto.Code, cancellationToken);

        if (!string.Equals(userInfo.Email, user.Email, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Provider email does not match your registered email");

        var existing = await _authRepo.GetByUserIdAndProviderAsync(user.UserId, request.Dto.Provider);
        if (existing != null) throw new InvalidOperationException("Provider already linked to your account");

        var entity = new UserAuthProvider
        {
            Id = Guid.NewGuid(),
            UserId = user.UserId,
            AuthProvider = request.Dto.Provider,
            ExternalUserId = userInfo.ExternalUserId,
            CreatedOn = DateTime.UtcNow
        };

        await _authRepo.AddAsync(entity);
        return true;
    }

    private IOAuthService GetServiceForProvider(AuthProviderEnum provider)
    {
        return _oauthServices.FirstOrDefault(x => x.Provider == provider)
            ?? throw new InvalidOperationException("Unsupported provider");
    }
}
