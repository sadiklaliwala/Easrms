using Easrms.Application.DTOs.User;
using Easrms.Application.Features.User.Commands;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Domain.Entities;
using FluentAssertions;
using MediatR;
using Moq;

namespace Easrms.Test.features.Users.Commands;

public sealed class UpdateUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly UpdateUserCommandHandler _handler;

    public UpdateUserCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _handler = new UpdateUserCommandHandler(_userRepositoryMock.Object);
    }

    private void SetupGetById(Guid userId, Domain.Entities.User? user)
    {
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
    }

    private void SetupEmailExists(string email, Guid excludeUserId, bool exists)
    {
        _userRepositoryMock
            .Setup(r => r.EmailExistsAsync(email, excludeUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
    }

    private void SetupManagerExists(Guid managerId, bool exists)
    {
        _userRepositoryMock
            .Setup(r => r.ExistsAsync(managerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
    }

    private void SetupUpdateAsync()
    {
        _userRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Domain.Entities.User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private static Domain.Entities.User BuildExistingUser(Guid userId) => new()
    {
        UserId = userId,
        FullName = "Old Name",
        Email = "old@example.com",
        RoleId = Guid.NewGuid(),
        IsActive = true,
        CreatedOn = DateTime.UtcNow.AddDays(-5)
    };

    private static UpdateUserDto BuildDto(Guid? managerId = null) => new()
    {
        FullName = "  Alice Johnson  ",
        Email = "  Alice@Example.COM  ",
        RoleId = Guid.NewGuid(),
        ManagerId = managerId
    };

    // ─── Success ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Should_ReturnUnit_When_UserUpdatedSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var managerId = Guid.NewGuid();
        var dto = BuildDto(managerId);
        var existing = BuildExistingUser(userId);
        var command = new UpdateUserCommand(userId, dto);

        SetupGetById(userId, existing);
        SetupEmailExists(dto.Email, userId, false);
        SetupManagerExists(managerId, true);
        SetupUpdateAsync();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
    }

    [Fact]
    public async Task Should_TrimAndLowercaseEmail_When_UserUpdated()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var managerId = Guid.NewGuid();
        var dto = BuildDto(managerId);
        var existing = BuildExistingUser(userId);
        var command = new UpdateUserCommand(userId, dto);

        SetupGetById(userId, existing);
        SetupEmailExists(dto.Email, userId, false);
        SetupManagerExists(managerId, true);

        Domain.Entities.User? captured = null;
        _userRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Domain.Entities.User>(), It.IsAny<CancellationToken>()))
            .Callback<Domain.Entities.User, CancellationToken>((u, _) => captured = u)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        captured!.Email.Should().Be("alice@example.com");
    }

    [Fact]
    public async Task Should_TrimFullName_When_UserUpdated()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var managerId = Guid.NewGuid();
        var dto = BuildDto(managerId);
        var existing = BuildExistingUser(userId);
        var command = new UpdateUserCommand(userId, dto);

        SetupGetById(userId, existing);
        SetupEmailExists(dto.Email, userId, false);
        SetupManagerExists(managerId, true);

        Domain.Entities.User? captured = null;
        _userRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Domain.Entities.User>(), It.IsAny<CancellationToken>()))
            .Callback<Domain.Entities.User, CancellationToken>((u, _) => captured = u)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        captured!.FullName.Should().Be("Alice Johnson");
    }

    [Fact]
    public async Task Should_SetUpdatedOn_When_UserUpdated()
    {
        // Arrange
        var before = DateTime.UtcNow.AddSeconds(-1);
        var userId = Guid.NewGuid();
        var managerId = Guid.NewGuid();
        var dto = BuildDto(managerId);
        var existing = BuildExistingUser(userId);
        var command = new UpdateUserCommand(userId, dto);

        SetupGetById(userId, existing);
        SetupEmailExists(dto.Email, userId, false);
        SetupManagerExists(managerId, true);

        Domain.Entities.User? captured = null;
        _userRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Domain.Entities.User>(), It.IsAny<CancellationToken>()))
            .Callback<Domain.Entities.User, CancellationToken>((u, _) => captured = u)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        captured!.UpdatedOn.Should().BeAfter(before);
    }

    [Fact]
    public async Task Should_NotCheckManagerExists_When_ManagerIdIsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto = BuildDto(managerId: null);
        var existing = BuildExistingUser(userId);
        var command = new UpdateUserCommand(userId, dto);

        SetupGetById(userId, existing);
        SetupEmailExists(dto.Email, userId, false);
        SetupUpdateAsync();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(
            r => r.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ─── User not found ──────────────────────────────────────────────────────

    [Fact]
    public async Task Should_ThrowKeyNotFoundException_When_UserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand(userId, BuildDto());

        SetupGetById(userId, null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Should_NeverCallUpdateAsync_When_UserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand(userId, BuildDto());

        SetupGetById(userId, null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();

        // Assert
        _userRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Domain.Entities.User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ─── Email conflict ───────────────────────────────────────────────────────

    [Fact]
    public async Task Should_ThrowInvalidOperationException_When_EmailAlreadyExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto = BuildDto();
        var existing = BuildExistingUser(userId);
        var command = new UpdateUserCommand(userId, dto);

        SetupGetById(userId, existing);
        SetupEmailExists(dto.Email, userId, true);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task Should_NeverCallUpdateAsync_When_EmailAlreadyExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto = BuildDto();
        var existing = BuildExistingUser(userId);
        var command = new UpdateUserCommand(userId, dto);

        SetupGetById(userId, existing);
        SetupEmailExists(dto.Email, userId, true);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();

        // Assert
        _userRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Domain.Entities.User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ─── Manager not found ───────────────────────────────────────────────────

    [Fact]
    public async Task Should_ThrowKeyNotFoundException_When_ManagerNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var managerId = Guid.NewGuid();
        var dto = BuildDto(managerId);
        var existing = BuildExistingUser(userId);
        var command = new UpdateUserCommand(userId, dto);

        SetupGetById(userId, existing);
        SetupEmailExists(dto.Email, userId, false);
        SetupManagerExists(managerId, false);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Manager*");
    }

    [Fact]
    public async Task Should_NeverCallUpdateAsync_When_ManagerNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var managerId = Guid.NewGuid();
        var dto = BuildDto(managerId);
        var existing = BuildExistingUser(userId);
        var command = new UpdateUserCommand(userId, dto);

        SetupGetById(userId, existing);
        SetupEmailExists(dto.Email, userId, false);
        SetupManagerExists(managerId, false);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();

        // Assert
        _userRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Domain.Entities.User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ─── Repository call verification ────────────────────────────────────────

    [Fact]
    public async Task Should_CallGetByIdAsync_Once_When_Handled()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var managerId = Guid.NewGuid();
        var dto = BuildDto(managerId);
        var existing = BuildExistingUser(userId);
        var command = new UpdateUserCommand(userId, dto);

        SetupGetById(userId, existing);
        SetupEmailExists(dto.Email, userId, false);
        SetupManagerExists(managerId, true);
        SetupUpdateAsync();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(
            r => r.GetByIdAsync(userId, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_CallEmailExistsAsync_Once_When_UserFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var managerId = Guid.NewGuid();
        var dto = BuildDto(managerId);
        var existing = BuildExistingUser(userId);
        var command = new UpdateUserCommand(userId, dto);

        SetupGetById(userId, existing);
        SetupEmailExists(dto.Email, userId, false);
        SetupManagerExists(managerId, true);
        SetupUpdateAsync();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(
            r => r.EmailExistsAsync(dto.Email, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_CallUpdateAsync_Once_When_AllChecksPass()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var managerId = Guid.NewGuid();
        var dto = BuildDto(managerId);
        var existing = BuildExistingUser(userId);
        var command = new UpdateUserCommand(userId, dto);

        SetupGetById(userId, existing);
        SetupEmailExists(dto.Email, userId, false);
        SetupManagerExists(managerId, true);
        SetupUpdateAsync();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Domain.Entities.User>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}