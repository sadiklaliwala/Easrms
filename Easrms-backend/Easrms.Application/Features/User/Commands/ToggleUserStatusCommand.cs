using Easrms.Application.Interfaces.Repositories;
using MediatR;

namespace Easrms.Application.Features.User.Commands;

public sealed record ToggleUserStatusCommand(
    Guid UserId
) : IRequest<Unit>;



/// <summary>
/// Handles activation/deactivation of a user.
/// </summary>
public sealed class ToggleUserStatusCommandHandler
    : IRequestHandler<ToggleUserStatusCommand, Unit>
{
    private readonly IUserRepository _userRepository;

    public ToggleUserStatusCommandHandler(
        IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Unit> Handle(
        ToggleUserStatusCommand request,
        CancellationToken cancellationToken)
    {
        // ---------------------------------------------------------------------
        // GET EXISTING USER
        // ---------------------------------------------------------------------

        var user = await _userRepository.GetByIdAsync(
            request.UserId,
            trackChanges: true,
            cancellationToken);

        if (user is null)
        {
            throw new KeyNotFoundException(
                "User not found.");
        }

        // ---------------------------------------------------------------------
        // TOGGLE STATUS
        // ---------------------------------------------------------------------

        user.IsActive = !user.IsActive;
        user.UpdatedOn = DateTime.UtcNow;

        // ---------------------------------------------------------------------
        // UPDATE
        // ---------------------------------------------------------------------

        await _userRepository.UpdateAsync(
            user,
            cancellationToken);

        // ---------------------------------------------------------------------
        // RETURN
        // ---------------------------------------------------------------------

        return Unit.Value;
    }
}