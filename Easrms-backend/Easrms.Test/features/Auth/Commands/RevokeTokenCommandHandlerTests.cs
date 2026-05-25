using Easrms.Application.Features.Auth.Commands;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace Easrms.Test.features.Auth.Commands;

public sealed class RevokeTokenCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly RevokeTokenCommandHandler _handler;

    public RevokeTokenCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();

        _handler = new RevokeTokenCommandHandler(
            _userRepositoryMock.Object);
    }

    // ─────────────────────────────────────────────────────────────
    // SUCCESS
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Should_RevokeTokenSuccessfully_When_ValidRefreshTokenProvided()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var refreshToken = "valid-refresh-token";
        var user = new User { UserId = userId };

        _userRepositoryMock
            .Setup(r => r.GetByRefreshTokenAsync(
                refreshToken,
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(r => r.RevokeRefreshTokenAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new RevokeTokenCommand { RefreshToken = refreshToken };

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Should_CallRevokeRefreshTokenAsync_WithCorrectUserId_When_TokenIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var refreshToken = "valid-refresh-token";
        var user = new User { UserId = userId };

        _userRepositoryMock
            .Setup(r => r.GetByRefreshTokenAsync(
                refreshToken,
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(r => r.RevokeRefreshTokenAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new RevokeTokenCommand { RefreshToken = refreshToken };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(
            r => r.RevokeRefreshTokenAsync(
                userId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_CallGetByRefreshTokenAsync_WithTrackChangesFalse_When_TokenIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var refreshToken = "valid-refresh-token";
        var user = new User { UserId = userId };

        _userRepositoryMock
            .Setup(r => r.GetByRefreshTokenAsync(
                refreshToken,
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(r => r.RevokeRefreshTokenAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new RevokeTokenCommand { RefreshToken = refreshToken };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert — must use trackChanges: false (no EF tracking on read)
        _userRepositoryMock.Verify(
            r => r.GetByRefreshTokenAsync(
                refreshToken,
                false,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _userRepositoryMock.Verify(
            r => r.GetByRefreshTokenAsync(
                refreshToken,
                true,
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Should_CallGetByRefreshTokenAsync_Once_When_TokenIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var refreshToken = "valid-refresh-token";
        var user = new User { UserId = userId };

        _userRepositoryMock
            .Setup(r => r.GetByRefreshTokenAsync(
                refreshToken,
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(r => r.RevokeRefreshTokenAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new RevokeTokenCommand { RefreshToken = refreshToken };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(
            r => r.GetByRefreshTokenAsync(
                refreshToken,
                false,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ─────────────────────────────────────────────────────────────
    // NOT FOUND / UNAUTHORIZED
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Should_ThrowUnauthorizedAccessException_When_RefreshTokenNotFound()
    {
        // Arrange
        var refreshToken = "expired-or-invalid-token";

        _userRepositoryMock
            .Setup(r => r.GetByRefreshTokenAsync(
                refreshToken,
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new RevokeTokenCommand { RefreshToken = refreshToken };

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Refresh token is invalid or has already been revoked.");
    }

    [Fact]
    public async Task Should_ThrowUnauthorizedAccessException_When_RefreshTokenIsEmpty()
    {
        // Arrange
        var refreshToken = string.Empty;

        _userRepositoryMock
            .Setup(r => r.GetByRefreshTokenAsync(
                refreshToken,
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new RevokeTokenCommand { RefreshToken = refreshToken };

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Refresh token is invalid or has already been revoked.");
    }

    [Fact]
    public async Task Should_ThrowUnauthorizedAccessException_When_RefreshTokenAlreadyRevoked()
    {
        // Arrange — simulates a token that was previously revoked (null returned from repo)
        var refreshToken = "already-revoked-token";

        _userRepositoryMock
            .Setup(r => r.GetByRefreshTokenAsync(
                refreshToken,
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new RevokeTokenCommand { RefreshToken = refreshToken };

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Refresh token is invalid or has already been revoked.");
    }

    [Fact]
    public async Task Should_NotCallRevokeRefreshTokenAsync_When_RefreshTokenNotFound()
    {
        // Arrange
        var refreshToken = "invalid-token";

        _userRepositoryMock
            .Setup(r => r.GetByRefreshTokenAsync(
                refreshToken,
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new RevokeTokenCommand { RefreshToken = refreshToken };

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();

        // Assert
        _userRepositoryMock.Verify(
            r => r.RevokeRefreshTokenAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ─────────────────────────────────────────────────────────────
    // NO EXTRA WRITE METHODS CALLED
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Should_NotCallAnyOtherWriteMethod_When_RevokeSucceeds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var refreshToken = "valid-refresh-token";
        var user = new User { UserId = userId };

        _userRepositoryMock
            .Setup(r => r.GetByRefreshTokenAsync(
                refreshToken,
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(r => r.RevokeRefreshTokenAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new RevokeTokenCommand { RefreshToken = refreshToken };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert — no other write methods should be touched
        _userRepositoryMock.Verify(
            r => r.CreateAsync(
                It.IsAny<User>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _userRepositoryMock.Verify(
            r => r.UpdateAsync(
                It.IsAny<User>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _userRepositoryMock.Verify(
            r => r.ToggleStatusAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _userRepositoryMock.Verify(
            r => r.UpdateLastLoginAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _userRepositoryMock.Verify(
            r => r.UpdateRefreshTokenAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}