using MediatR;

namespace Easrms.Application.Features.Auth.Commands.UnlinkProvider;

public sealed record UnlinkProviderCommand(
    Guid ProviderId,
    Guid CurrentUserId
) : IRequest<bool>;
