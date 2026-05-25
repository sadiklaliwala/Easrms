using Easrms.Application.DTOs.Auth;
using Easrms.Application.Interfaces.OAuth;
using MediatR;

namespace Easrms.Application.Features.Auth.Queries.GetLinkedProviders;

public class GetLinkedProvidersQueryHandler : IRequestHandler<GetLinkedProvidersQuery, List<LinkedProviderDto>>
{
    private readonly IAuthProviderRepository _authRepo;

    public GetLinkedProvidersQueryHandler(IAuthProviderRepository authRepo)
    {
        _authRepo = authRepo;
    }

    public async Task<List<LinkedProviderDto>> Handle(GetLinkedProvidersQuery request, CancellationToken cancellationToken)
    {
        var rows = await _authRepo.GetByUserIdAsync(request.CurrentUserId);
        return rows.Select(r => new LinkedProviderDto
        {
            Id = r.Id,
            AuthProvider = r.AuthProvider,
            ProviderName = r.AuthProvider.ToString(),
            LinkedOn = r.CreatedOn
        }).ToList();
    }
}
