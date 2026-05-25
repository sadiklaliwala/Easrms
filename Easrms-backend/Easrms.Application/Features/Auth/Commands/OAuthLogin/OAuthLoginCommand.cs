using Easrms.Application.DTOs.Auth;
using Easrms.Common.Constants;
using MediatR;

namespace Easrms.Application.Features.Auth.Commands.OAuthLogin;

public sealed record OAuthLoginCommand(
    OAuthLoginDto Dto
) : IRequest<LoginResponseDto>;
