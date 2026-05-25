// FileName: GetCurrentUserQueryHandlerTests.cs

using AutoMapper;
using Easrms.Application.DTOs.Auth;
using Easrms.Application.Features.Auth.Queries;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Domain.Entities;
using FluentAssertions;
using Moq;

namespace Easrms.Test.features.Auth.Queries;

public sealed class GetCurrentUserQueryHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetCurrentUserQueryHandler _handler;

    public GetCurrentUserQueryHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _mapperMock = new Mock<IMapper>();
        _handler = new GetCurrentUserQueryHandler(
            _userRepositoryMock.Object,
            _mapperMock.Object);
    }

    // ─── Helpers ────────────────────────────────────────────────────────────

    private static User CreateUser(Guid? id = null) => new()
    {
        UserId = id ?? Guid.NewGuid(),
        FullName = "John Doe",
        Email = "john@example.com"
    };

    private static CurrentUserDto CreateCurrentUserDto(User user) => new()
    {
        UserId = user.UserId,
        FullName = user.FullName,
        Email = user.Email,
        RoleName = "Employee"
    };

    private void SetupGetByIdReturns(Guid userId, User? user)
    {
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
    }

    // ─── Success ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Should_ReturnCurrentUserDto_When_UserExists()
    {
        // Arrange
        var user = CreateUser();
        var expectedDto = CreateCurrentUserDto(user);
        var query = new GetCurrentUserQuery { CurrentUserId = user.UserId };

        SetupGetByIdReturns(user.UserId, user);
        _mapperMock
            .Setup(m => m.Map<CurrentUserDto>(user))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedDto);
    }

    [Fact]
    public async Task Should_ReturnCorrectUserId_When_UserExists()
    {
        // Arrange
        var user = CreateUser();
        var expectedDto = CreateCurrentUserDto(user);
        var query = new GetCurrentUserQuery { CurrentUserId = user.UserId };

        SetupGetByIdReturns(user.UserId, user);
        _mapperMock
            .Setup(m => m.Map<CurrentUserDto>(user))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.UserId.Should().Be(user.UserId);
    }

    // ─── Unauthorized ────────────────────────────────────────────────────────

    [Fact]
    public async Task Should_ThrowUnauthorizedAccessException_When_UserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetCurrentUserQuery { CurrentUserId = userId };

        SetupGetByIdReturns(userId, null);

        // Act
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User not found.");
    }

    [Fact]
    public async Task Should_ThrowUnauthorizedAccessException_When_UserIdIsEmpty()
    {
        // Arrange
        var emptyId = Guid.Empty;
        var query = new GetCurrentUserQuery { CurrentUserId = emptyId };

        SetupGetByIdReturns(emptyId, null);

        // Act
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User not found.");
    }

    // ─── Repository call verification ────────────────────────────────────────

    [Fact]
    public async Task Should_CallGetByIdAsync_WithCorrectParameters_When_Handled()
    {
        // Arrange
        var user = CreateUser();
        var query = new GetCurrentUserQuery { CurrentUserId = user.UserId };

        SetupGetByIdReturns(user.UserId, user);
        _mapperMock
            .Setup(m => m.Map<CurrentUserDto>(user))
            .Returns(CreateCurrentUserDto(user));

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(
            r => r.GetByIdAsync(user.UserId, false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_CallGetByIdAsync_WithTrackChangesFalse_When_Handled()
    {
        // Arrange
        var user = CreateUser();
        var query = new GetCurrentUserQuery { CurrentUserId = user.UserId };

        SetupGetByIdReturns(user.UserId, user);
        _mapperMock
            .Setup(m => m.Map<CurrentUserDto>(user))
            .Returns(CreateCurrentUserDto(user));

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ─── Mapper call verification ─────────────────────────────────────────────

    [Fact]
    public async Task Should_CallMapper_Once_When_UserExists()
    {
        // Arrange
        var user = CreateUser();
        var query = new GetCurrentUserQuery { CurrentUserId = user.UserId };

        SetupGetByIdReturns(user.UserId, user);
        _mapperMock
            .Setup(m => m.Map<CurrentUserDto>(user))
            .Returns(CreateCurrentUserDto(user));

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mapperMock.Verify(
            m => m.Map<CurrentUserDto>(user),
            Times.Once);
    }

    [Fact]
    public async Task Should_NeverCallMapper_When_UserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetCurrentUserQuery { CurrentUserId = userId };

        SetupGetByIdReturns(userId, null);

        // Act
        var act = async () => await _handler.Handle(query, CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();

        // Assert
        _mapperMock.Verify(
            m => m.Map<CurrentUserDto>(It.IsAny<User>()),
            Times.Never);
    }
}



