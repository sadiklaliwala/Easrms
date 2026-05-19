using Easrms.Application.DTOs.User;
using Easrms.Application.Interfaces.Repositories;
using MediatR;

namespace Easrms.Application.Features.User.Commands;

public sealed record UpdateUserCommand(
    Guid UserId,
    UpdateUserDto Dto
) : IRequest<Unit>;



/// <summary>
/// Handles updating an existing user.
/// </summary>
public sealed class UpdateUserCommandHandler
    : IRequestHandler<UpdateUserCommand, Unit>
{
    private readonly IUserRepository _userRepository;

    public UpdateUserCommandHandler(
        IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Unit> Handle(
        UpdateUserCommand request,
        CancellationToken cancellationToken)
    {
        // ---------------------------------------------------------------------
        // DTO
        // ---------------------------------------------------------------------

        var dto = request.Dto;

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
        // CHECK — EMAIL ALREADY EXISTS
        // ---------------------------------------------------------------------

        var emailExists = await _userRepository.EmailExistsAsync(
            dto.Email,
            excludeUserId: request.UserId,
            cancellationToken);

        if (emailExists)
        {
            throw new InvalidOperationException(
                "Email already exists.");
        }

        // ---------------------------------------------------------------------
        // CHECK — MANAGER EXISTS
        // ---------------------------------------------------------------------

        if (dto.ManagerId.HasValue)
        {
            var managerExists = await _userRepository.ExistsAsync(
                dto.ManagerId.Value,
                cancellationToken);

            if (!managerExists)
            {
                throw new KeyNotFoundException(
                    "Manager not found.");
            }
        }

        // ---------------------------------------------------------------------
        // APPLY CHANGES
        // ---------------------------------------------------------------------

        user.FullName = dto.FullName.Trim();
        user.Email = dto.Email.Trim().ToLower();
        user.RoleId = dto.RoleId;
        user.ManagerId = dto.ManagerId;
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