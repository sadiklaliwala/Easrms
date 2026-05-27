using System;
using System.Threading;
using System.Threading.Tasks;
using Easrms.Application.Features.Auth.Commands;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Domain.Entities;
using Easrms.Application.DTOs.Auth;
using FluentAssertions;
using Moq;
using Xunit;
using Easrms.Application.Interfaces.Jwt;

namespace Easrms.Test.features.Auth.Commands;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IJwtService> _jwtService = new();
    private readonly Mock<IJwtSettings> _jwtSettings = new();
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _handler = new LoginCommandHandler(_userRepo.Object, _jwtService.Object, _jwtSettings.Object);
    }

    [Fact]
    public async Task Should_ReturnUser_When_CredentialsValid()
    {
        var email = "user@example.com";
        var password = "Secret123";
        var user = new User { UserId = Guid.NewGuid(), Email = email, IsActive = true, PasswordHash = BCrypt.Net.BCrypt.Hash(password), FullName = "User", Role = new Role { RoleName = "Employee" } };
        _userRepo.Setup(u => u.GetByEmailAsync(email, false, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _jwtService.Setup(j => j.GenerateAccessToken(user)).Returns("access-token");
        _jwtService.Setup(j => j.GenerateRefreshToken()).Returns("refresh-token");
        _jwtService.Setup(j => j.SetTokenCookie(It.IsAny<string>()));
        _jwtService.Setup(j => j.SetRefreshTokenCookie(It.IsAny<string>()));
        _userRepo.Setup(u => u.UpdateLoginMetaAsync(user.UserId, It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var command = new LoginCommand { Email = email, Password = password };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Email.Should().Be(email);
        _userRepo.Verify(u => u.UpdateLoginMetaAsync(user.UserId, It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_ThrowUnauthorized_When_UserNotFound()
    {
        _userRepo.Setup(u => u.GetByEmailAsync(It.IsAny<string>(), false, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        var command = new LoginCommand { Email = "nope", Password = "x" };
        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None)).Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Should_ThrowUnauthorized_When_PasswordInvalid()
    {
        var email = "user@example.com";
        var user = new User { UserId = Guid.NewGuid(), Email = email, IsActive = true, PasswordHash = BCrypt.Net.BCrypt.Hash("other") };
        _userRepo.Setup(u => u.GetByEmailAsync(email, false, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        var command = new LoginCommand { Email = email, Password = "wrong" };
        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None)).Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Should_ThrowUnauthorized_When_Inactive()
    {
        var email = "user@example.com";
        var user = new User { UserId = Guid.NewGuid(), Email = email, IsActive = false, PasswordHash = BCrypt.Net.BCrypt.Hash("pass") };
        _userRepo.Setup(u => u.GetByEmailAsync(email, false, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        var command = new LoginCommand { Email = email, Password = "pass" };
        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None)).Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
