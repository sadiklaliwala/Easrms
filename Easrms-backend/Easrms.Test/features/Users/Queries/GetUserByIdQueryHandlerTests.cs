using AutoMapper;
using Easrms.Application.DTOs.User;
using Easrms.Application.Features.User.Queries;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Domain.Entities;
using FluentAssertions;
using Moq;

namespace Easrms.Test.features.Users.Queries;

public sealed class GetUserByIdQueryHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetUserByIdQueryHandler _handler;

    public GetUserByIdQueryHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _mapperMock = new Mock<IMapper>();
        _handler = new GetUserByIdQueryHandler(
            _userRepositoryMock.Object,
            _mapperMock.Object);
    }

    private void SetupGetById(Guid userId, User? user)
    {
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
    }

    private void SetupMapper(User user, UserDetailDto dto)
    {
        _mapperMock
            .Setup(m => m.Map<UserDetailDto>(user))
            .Returns(dto);
    }

    private static User BuildUser(Guid userId) => new()
    {
        UserId = userId,
        FullName = "Alice Johnson",
        Email = "alice@example.com",
        IsActive = true,
        CreatedOn = DateTime.UtcNow.AddDays(-10)
    };

    private static UserDetailDto BuildDetailDto(Guid userId) => new()
    {
        UserId = userId,
        FullName = "Alice Johnson",
        Email = "alice@example.com",
        IsActive = true
    };

    // ─── Success ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Should_ReturnUserDetailDto_When_UserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = BuildUser(userId);
        var expected = BuildDetailDto(userId);
        var query = new GetUserByIdQuery(userId);

        SetupGetById(userId, user);
        SetupMapper(user, expected);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Should_ReturnMappedDto_With_CorrectUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = BuildUser(userId);
        var expected = BuildDetailDto(userId);
        var query = new GetUserByIdQuery(userId);

        SetupGetById(userId, user);
        SetupMapper(user, expected);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.UserId.Should().Be(userId);
    }

    // ─── Not found ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Should_ThrowKeyNotFoundException_When_UserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);

        SetupGetById(userId, null);

        // Act
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{userId}*");
    }

    [Fact]
    public async Task Should_NeverCallMapper_When_UserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);

        SetupGetById(userId, null);

        // Act
        var act = async () => await _handler.Handle(query, CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();

        // Assert
        _mapperMock.Verify(
            m => m.Map<UserDetailDto>(It.IsAny<User>()),
            Times.Never);
    }

    // ─── Repository call verification ────────────────────────────────────────

    [Fact]
    public async Task Should_CallGetByIdAsync_Once_WithTrackChangesFalse_When_Handled()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = BuildUser(userId);
        var expected = BuildDetailDto(userId);
        var query = new GetUserByIdQuery(userId);

        SetupGetById(userId, user);
        SetupMapper(user, expected);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(
            r => r.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_CallMapper_Once_When_UserFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = BuildUser(userId);
        var expected = BuildDetailDto(userId);
        var query = new GetUserByIdQuery(userId);

        SetupGetById(userId, user);
        SetupMapper(user, expected);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mapperMock.Verify(
            m => m.Map<UserDetailDto>(user),
            Times.Once);
    }
}