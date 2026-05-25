using Easrms.Application.DTOs.Auth;
using MediatR;

namespace Easrms.Application.Features.Auth.Queries.GetLinkedProviders;

public sealed record GetLinkedProvidersQuery(Guid CurrentUserId) : IRequest<List<LinkedProviderDto>>;
