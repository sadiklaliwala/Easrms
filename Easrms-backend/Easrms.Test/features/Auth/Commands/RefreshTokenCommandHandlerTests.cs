using System;
using System.Threading;
using System.Threading.Tasks;
using Easrms.Application.Features.Auth.Commands;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Application.Interfaces;
using Easrms.Domain.Entities;
using Easrms.Application.DTOs.Auth;
using FluentAssertions;
using Moq;
using Xunit;

namespace Easrms.Test.features.Auth.Commands;

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IJwtService> _jwtService = new();
    private readonly Mock<IJwtSettings> _jwtSettings = new();
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _handler = new RefreshTokenCommandHandler(_userRepo.Object, _jwtService.Object, _jwtSettings.Object);
    }

    [Fact]
    public async Task Should_Refresh_When_TokenValid()
    {
        var user = new User { UserId = Guid.NewGuid(), RefreshToken = "old", RefreshTokenExpiryOn = DateTime.UtcNow.AddHours(1) };
        _jwtService.Setup(j => j.GetRefreshTokenFromCookie()).Returns("old");
        _userRepo.Setup(u => u.GetByRefreshTokenAsync("old", false, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _jwtService.Setup(j => j.GenerateAccessToken(user)).Returns("newacc");
        _jwtService.Setup(j => j.GenerateRefreshToken()).Returns("newref");
        _userRepo.Setup(u => u.UpdateRefreshTokenAsync(user.UserId, It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var command = new RefreshTokenCommand { RefreshToken = "old" };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.AccessToken.Should().Be("newacc");
    }

    [Fact]
    public async Task Should_ThrowUnauthorized_When_NoCookie()
    {
        _jwtService.Setup(j => j.GetRefreshTokenFromCookie()).Returns(string.Empty);
        var command = new RefreshTokenCommand { RefreshToken = "x" };
        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None)).Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Should_ThrowUnauthorized_When_TokenInvalid()
    {
        _jwtService.Setup(j => j.GetRefreshTokenFromCookie()).Returns("bad");
        _userRepo.Setup(u => u.GetByRefreshTokenAsync("bad", false, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        var command = new RefreshTokenCommand { RefreshToken = "bad" };
        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None)).Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Should_ThrowUnauthorized_When_Expired()
    {
        var user = new User { UserId = Guid.NewGuid(), RefreshToken = "old", RefreshTokenExpiryOn = DateTime.UtcNow.AddDays(-1) };
        _jwtService.Setup(j => j.GetRefreshTokenFromCookie()).Returns("old");
        _userRepo.Setup(u => u.GetByRefreshTokenAsync("old", false, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        var command = new RefreshTokenCommand { RefreshToken = "old" };
        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None)).Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
