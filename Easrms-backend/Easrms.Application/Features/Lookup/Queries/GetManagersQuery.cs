using Easrms.Application.DTOs.Lookup;
using Easrms.Application.Interfaces.Repositories;
using MediatR;

namespace Easrms.Application.Features.Lookup.Queries;

/// <summary>
/// Query to retrieve all active managers for the manager dropdown on
/// Create User and Edit User screens.
/// No parameters needed — the repository always returns all active managers.
/// Only Admin callers reach this endpoint (enforced at controller level).
/// </summary>
public record GetManagersQuery : IRequest<IReadOnlyList<ManagerLookupDto>>;


/// <summary>
/// Handles <see cref="GetManagersQuery"/>.
///
/// Flow:
///   1. Delegate directly to <see cref="ILookupRepository.GetActiveManagersAsync"/>.
///   2. Return the flat list as-is — no mapping needed, repository returns DTOs directly.
///
/// No guards needed — this is a read-only dropdown lookup with no resource ownership.
/// Empty list is a valid response when no active managers exist.
/// </summary>
public class GetManagersQueryHandler
    : IRequestHandler<GetManagersQuery, IReadOnlyList<ManagerLookupDto>>
{
    private readonly ILookupRepository _lookupRepository;

    public GetManagersQueryHandler(ILookupRepository lookupRepository)
    {
        _lookupRepository = lookupRepository;
    }

    public async Task<IReadOnlyList<ManagerLookupDto>> Handle(
        GetManagersQuery request,
        CancellationToken cancellationToken)
    {
        return await _lookupRepository.GetActiveManagersAsync(cancellationToken);
    }
}