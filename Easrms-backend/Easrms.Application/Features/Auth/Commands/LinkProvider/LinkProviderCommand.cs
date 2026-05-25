using Easrms.Application.DTOs.Auth;
using Easrms.Common.Constants;
using MediatR;

namespace Easrms.Application.Features.Auth.Commands.LinkProvider;

public sealed record LinkProviderCommand(
    LinkProviderDto Dto,
    Guid CurrentUserId
) : IRequest<bool>;
