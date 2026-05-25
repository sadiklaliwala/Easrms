using Easrms.Application.Interfaces.OAuth;
using Easrms.Application.Interfaces.Repositories;
using MediatR;

namespace Easrms.Application.Features.Auth.Commands.UnlinkProvider;

public class UnlinkProviderCommandHandler : IRequestHandler<UnlinkProviderCommand, bool>
{
    private readonly IAuthProviderRepository _authRepo;
    private readonly IUserRepository _userRepo;

    public UnlinkProviderCommandHandler(IAuthProviderRepository authRepo, IUserRepository userRepo)
    {
        _authRepo = authRepo;
        _userRepo = userRepo;
    }

    public async Task<bool> Handle(UnlinkProviderCommand request, CancellationToken cancellationToken)
    {
        var count = await _authRepo.CountByUserIdAsync(request.CurrentUserId);
        if (count <= 1)
            throw new InvalidOperationException("Cannot unlink only remaining provider");

        var entity = (await _authRepo.GetByUserIdAsync(request.CurrentUserId)).FirstOrDefault(x => x.Id == request.ProviderId);
        if (entity == null)
            throw new KeyNotFoundException("Provider not found");

        await _authRepo.DeleteAsync(request.ProviderId);
        return true;
    }
}
