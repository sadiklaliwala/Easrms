using Easrms.Application.DTOs.Lookup;
using Easrms.Application.Interfaces.Repositories;
using MediatR;

namespace Easrms.Application.Features.Lookup.Queries;

/// <summary>
/// Query to retrieve all active support users for the assign-request dropdown.
/// No parameters needed — the repository always returns all active support users.
/// Only Admin callers reach this endpoint (enforced at controller level).
/// </summary>
public record GetSupportUsersQuery : IRequest<IReadOnlyList<SupportUserLookupDto>>;


/// <summary>
/// Handles <see cref="GetSupportUsersQuery"/>.
///
/// Flow:
///   1. Delegate directly to <see cref="ILookupRepository.GetActiveSupportUsersAsync"/>.
///   2. Return the flat list as-is — no mapping needed, repository returns DTOs directly.
///
/// No guards needed — this is a read-only dropdown lookup with no resource ownership.
/// Empty list is a valid response when no active support users exist.
/// </summary>
public class GetSupportUsersQueryHandler
    : IRequestHandler<GetSupportUsersQuery, IReadOnlyList<SupportUserLookupDto>>
{
    private readonly ILookupRepository _lookupRepository;

    public GetSupportUsersQueryHandler(ILookupRepository lookupRepository)
    {
        _lookupRepository = lookupRepository;
    }

    public async Task<IReadOnlyList<SupportUserLookupDto>> Handle(
        GetSupportUsersQuery request,
        CancellationToken cancellationToken)
    {
        return await _lookupRepository.GetActiveSupportUsersAsync(cancellationToken);
    }
}
