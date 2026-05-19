using AutoMapper;
using Easrms.Application.DTOs.Request;
using Easrms.Application.Interfaces.Repositories;
using MediatR;

namespace Easrms.Application.Features.Request.Queries;

/// <summary>
/// Returns the full detail of a single service request.
/// </summary>
public sealed class GetRequestByIdQuery : IRequest<RequestDetailDto>
{
    public Guid RequestId { get; init; }
}



/// <summary>
/// Step-by-step per HANDLER_REPO_REFERENCE_MAP:
///   1. GetRequestByIdAsync(requestId) → 404 if null
///   2. Map entity → RequestDetailDto → return
/// </summary>
public sealed class GetRequestByIdQueryHandler : IRequestHandler<GetRequestByIdQuery, RequestDetailDto>
{
    private readonly IRequestRepository _requestRepository;
    private readonly IMapper _mapper;

    public GetRequestByIdQueryHandler(
        IRequestRepository requestRepository,
        IMapper mapper)
    {
        _requestRepository = requestRepository;
        _mapper = mapper;
    }

    public async Task<RequestDetailDto> Handle(
        GetRequestByIdQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Fetch full entity with all navigation properties needed by the DTO
        var entity = await _requestRepository.GetRequestByIdAsync(
            request.RequestId,
            cancellationToken: cancellationToken)
            ?? throw new KeyNotFoundException(
                $"Service request with id '{request.RequestId}' was not found.");

        // 2. Map and return
        return _mapper.Map<RequestDetailDto>(entity);
    }
}