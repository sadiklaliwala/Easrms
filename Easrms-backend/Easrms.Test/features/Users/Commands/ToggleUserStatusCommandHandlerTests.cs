using Easrms.Application.Features.User.Commands;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Domain.Entities;
using FluentAssertions;
using MediatR;
using Moq;

namespace Easrms.Test.features.Users.Commands;

public sealed class ToggleUserStatusCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly ToggleUserStatusCommandHandler _handler;

    public ToggleUserStatusCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _handler = new ToggleUserStatusCommandHandler(_userRepositoryMock.Object);
    }

    private void SetupGetById(Guid userId, Domain.Entities.User? user)
    {
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
    }

    private void SetupUpdateAsync()
    {
        _userRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Domain.Entities.User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private static Domain.Entities.User BuildUser(bool isActive) => new()
    {
        UserId = Guid.NewGuid(),
        FullName = "Alice Johnson",
        Email = "alice@example.com",
        IsActive = isActive,
        CreatedOn = DateTime.UtcNow.AddDays(-10)
    };

    // ─── Success ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Should_ReturnUnit_When_UserToggled()
    {
        // Arrange
        var user = BuildUser(isActive: true);
        var command = new ToggleUserStatusCommand(user.UserId);

        SetupGetById(user.UserId, user);
        SetupUpdateAsync();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
    }

    [Fact]
    public async Task Should_DeactivateUser_When_UserIsCurrentlyActive()
    {
        // Arrange
        var user = BuildUser(isActive: true);
        var command = new ToggleUserStatusCommand(user.UserId);

        SetupGetById(user.UserId, user);

        Domain.Entities.User? captured = null;
        _userRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Domain.Entities.User>(), It.IsAny<CancellationToken>()))
            .Callback<Domain.Entities.User, CancellationToken>((u, _) => captured = u)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        captured!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Should_ActivateUser_When_UserIsCurrentlyInactive()
    {
        // Arrange
        var user = BuildUser(isActive: false);
        var command = new ToggleUserStatusCommand(user.UserId);

        SetupGetById(user.UserId, user);

        Domain.Entities.User? captured = null;
        _userRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Domain.Entities.User>(), It.IsAny<CancellationToken>()))
            .Callback<Domain.Entities.User, CancellationToken>((u, _) => captured = u)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        captured!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Should_SetUpdatedOn_When_UserToggled()
    {
        // Arrange
        var before = DateTime.UtcNow.AddSeconds(-1);
        var user = BuildUser(isActive: true);
        var command = new ToggleUserStatusCommand(user.UserId);

        SetupGetById(user.UserId, user);

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

    // ─── Not found ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Should_ThrowKeyNotFoundException_When_UserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ToggleUserStatusCommand(userId);

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
        var command = new ToggleUserStatusCommand(userId);

        SetupGetById(userId, null);

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
        var user = BuildUser(isActive: true);
        var command = new ToggleUserStatusCommand(user.UserId);

        SetupGetById(user.UserId, user);
        SetupUpdateAsync();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(
            r => r.GetByIdAsync(user.UserId, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_CallUpdateAsync_Once_When_UserFound()
    {
        // Arrange
        var user = BuildUser(isActive: true);
        var command = new ToggleUserStatusCommand(user.UserId);

        SetupGetById(user.UserId, user);
        SetupUpdateAsync();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Domain.Entities.User>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}