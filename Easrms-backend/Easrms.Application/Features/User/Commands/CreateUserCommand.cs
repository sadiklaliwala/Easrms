using Easrms.Application.DTOs.User;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Common.Helpers;
using MediatR;

namespace Easrms.Application.Features.User.Commands;

public sealed record CreateUserCommand(
    CreateUserDto Dto
) : IRequest<Guid>;

/// <summary>
/// Handles creation of a new system user.
/// </summary>
public sealed class CreateUserCommandHandler
    : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;

    public CreateUserCommandHandler(
        IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Guid> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        // ---------------------------------------------------------------------
        // DTO
        // ---------------------------------------------------------------------

        var dto = request.Dto;

        // ---------------------------------------------------------------------
        // CHECK — EMAIL ALREADY EXISTS
        // ---------------------------------------------------------------------

        var emailExists = await _userRepository.EmailExistsAsync(
            dto.Email,
            excludeUserId: null,
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
        // CREATE ENTITY
        // ---------------------------------------------------------------------

        var user = new Easrms.Domain.Entities.User
        {
            UserId = Guid.NewGuid(),
            FullName = dto.FullName.Trim(),
            Email = dto.Email.Trim().ToLower(),
            PasswordHash = PasswordHelper.Hash(dto.Password),
            RoleId = dto.RoleId,
            ManagerId = dto.ManagerId,
            IsActive = true,
            CreatedOn = DateTime.UtcNow
        };

        // ---------------------------------------------------------------------
        // SAVE
        // ---------------------------------------------------------------------

        await _userRepository.CreateAsync(
            user,
            cancellationToken);

        // ---------------------------------------------------------------------
        // RETURN CREATED USER ID
        // ---------------------------------------------------------------------

        return user.UserId;
    }
}