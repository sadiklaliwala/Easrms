using Easrms.Application.Features.Auth.Commands;
using Easrms.Application.Interfaces.Jwt;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Domain.Entities;
using FluentAssertions;
using Moq;

namespace Easrms.UnitTest.features.Auth.Commands
{
    public sealed class LogoutCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly LogoutCommandHandler _handler;

        public LogoutCommandHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _jwtServiceMock = new Mock<IJwtService>();

            _handler = new LogoutCommandHandler(
                _userRepositoryMock.Object,
                _jwtServiceMock.Object);
        }

        // ─────────────────────────────────────────────────────────────
        // SUCCESS
        // ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Should_LogoutSuccessfully_When_UserExists()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var user = new User { UserId = userId };

            _userRepositoryMock
                .Setup(r => r.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _userRepositoryMock
                .Setup(r => r.RevokeRefreshTokenAsync(userId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _jwtServiceMock.Setup(j => j.ClearTokenCookie());
            _jwtServiceMock.Setup(j => j.ClearRefreshTokenCookie());

            var command = new LogoutCommand { CurrentUserId = userId };

            // Act
            var act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task Should_CallRevokeRefreshTokenAsync_When_UserExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { UserId = userId };

            _userRepositoryMock
                .Setup(r => r.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _userRepositoryMock
                .Setup(r => r.RevokeRefreshTokenAsync(userId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var command = new LogoutCommand { CurrentUserId = userId };

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _userRepositoryMock.Verify(
                r => r.RevokeRefreshTokenAsync(userId, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Should_CallClearTokenCookie_When_UserExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { UserId = userId };

            _userRepositoryMock
                .Setup(r => r.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _userRepositoryMock
                .Setup(r => r.RevokeRefreshTokenAsync(userId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var command = new LogoutCommand { CurrentUserId = userId };

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _jwtServiceMock.Verify(j => j.ClearTokenCookie(), Times.Once);
        }

        [Fact]
        public async Task Should_CallClearRefreshTokenCookie_When_UserExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { UserId = userId };

            _userRepositoryMock
                .Setup(r => r.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _userRepositoryMock
                .Setup(r => r.RevokeRefreshTokenAsync(userId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var command = new LogoutCommand { CurrentUserId = userId };

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _jwtServiceMock.Verify(j => j.ClearRefreshTokenCookie(), Times.Once);
        }

        [Fact]
        public async Task Should_CallBothClearCookieMethods_When_LogoutSucceeds()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { UserId = userId };

            _userRepositoryMock
                .Setup(r => r.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _userRepositoryMock
                .Setup(r => r.RevokeRefreshTokenAsync(userId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var command = new LogoutCommand { CurrentUserId = userId };

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _jwtServiceMock.Verify(j => j.ClearTokenCookie(), Times.Once);
            _jwtServiceMock.Verify(j => j.ClearRefreshTokenCookie(), Times.Once);
        }

        // ─────────────────────────────────────────────────────────────
        // NOT FOUND / UNAUTHORIZED
        // ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Should_ThrowUnauthorizedAccessException_When_UserNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var command = new LogoutCommand { CurrentUserId = userId };

            // Act
            var act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should()
                .ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("User session not found.");
        }

        [Fact]
        public async Task Should_NotCallRevokeRefreshTokenAsync_When_UserNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _userRepositoryMock
                .Setup(r => r.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var command = new LogoutCommand { CurrentUserId = userId };

            // Act
            var act = async () => await _handler.Handle(command, CancellationToken.None);
            await act.Should().ThrowAsync<UnauthorizedAccessException>();

            // Assert
            _userRepositoryMock.Verify(
                r => r.RevokeRefreshTokenAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Should_NotCallClearTokenCookie_When_UserNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _userRepositoryMock
                .Setup(r => r.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var command = new LogoutCommand { CurrentUserId = userId };

            // Act
            var act = async () => await _handler.Handle(command, CancellationToken.None);
            await act.Should().ThrowAsync<UnauthorizedAccessException>();

            // Assert
            _jwtServiceMock.Verify(j => j.ClearTokenCookie(), Times.Never);
            _jwtServiceMock.Verify(j => j.ClearRefreshTokenCookie(), Times.Never);
        }

        [Fact]
        public async Task Should_ThrowUnauthorizedAccessException_When_UserIdIsEmpty()
        {
            // Arrange
            var emptyUserId = Guid.Empty;

            _userRepositoryMock
                .Setup(r => r.GetByIdAsync(emptyUserId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var command = new LogoutCommand { CurrentUserId = emptyUserId };

            // Act
            var act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should()
                .ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("User session not found.");
        }

        // ─────────────────────────────────────────────────────────────
        // REPOSITORY CALL VERIFICATION
        // ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Should_CallGetByIdAsync_WithCorrectUserId()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { UserId = userId };

            _userRepositoryMock
                .Setup(r => r.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _userRepositoryMock
                .Setup(r => r.RevokeRefreshTokenAsync(userId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var command = new LogoutCommand { CurrentUserId = userId };

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _userRepositoryMock.Verify(
                r => r.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Should_CallGetByIdAsync_WithTrackChangesFalse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { UserId = userId };

            _userRepositoryMock
                .Setup(r => r.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _userRepositoryMock
                .Setup(r => r.RevokeRefreshTokenAsync(userId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var command = new LogoutCommand { CurrentUserId = userId };

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _userRepositoryMock.Verify(
                r => r.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()),
                Times.Once);

            // Ensure it was NOT called with trackChanges: true
            _userRepositoryMock.Verify(
                r => r.GetByIdAsync(userId, true, It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Should_NotCallAnyWriteMethod_Other_Than_RevokeRefreshToken_When_LogoutSucceeds()
        {
            // Arrange — only RevokeRefreshTokenAsync should be called, no CreateAsync/UpdateAsync
            var userId = Guid.NewGuid();
            var user = new User { UserId = userId };

            _userRepositoryMock
                .Setup(r => r.GetByIdAsync(
                    userId,
                    false,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _userRepositoryMock
                .Setup(r => r.RevokeRefreshTokenAsync(
                    userId,
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var command = new LogoutCommand { CurrentUserId = userId };

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert — no other write methods should be called
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
        }
    }
}
