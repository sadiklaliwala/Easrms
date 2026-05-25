using Easrms.Application.DTOs.User;
using Easrms.Application.Features.User.Commands;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Domain.Entities;
using FluentAssertions;
using Moq;

namespace Easrms.Test.features.Users.Commands;

public sealed class CreateUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly CreateUserCommandHandler _handler;

    public CreateUserCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _handler = new CreateUserCommandHandler(_userRepositoryMock.Object);
    }

    private void SetupEmailExists(string email, bool exists)
    {
        _userRepositoryMock
            .Setup(r => r.EmailExistsAsync(email, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
    }

    private void SetupManagerExists(Guid managerId, bool exists)
    {
        _userRepositoryMock
            .Setup(r => r.ExistsAsync(managerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
    }

    private void SetupCreateAsync()
    {
        _userRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Domain.Entities.User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private static CreateUserDto BuildDto(Guid? managerId = null) => new()
    {
        FullName = "  Alice Johnson  ",
        Email = "  Alice@Example.COM  ",
        Password = "S3cur3P@ss!",
        RoleId = Guid.NewGuid(),
        ManagerId = managerId
    };

    // ─── Success ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Should_ReturnNewGuid_When_UserCreatedSuccessfully()
    {
        // Arrange
        var managerId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var dto = BuildDto(managerId);
        var command = new CreateUserCommand(dto, currentUserId);

        SetupEmailExists(dto.Email, false);
        SetupManagerExists(managerId, true);
        SetupCreateAsync();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Should_AssignCurrentUserAsManager_When_ManagerIdIsNull()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var dto = BuildDto(managerId: null);
        var command = new CreateUserCommand(dto, currentUserId);

        SetupEmailExists(dto.Email, false);
        SetupCreateAsync();

        Domain.Entities.User? capturedUser = null;
        _userRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Domain.Entities.User>(), It.IsAny<CancellationToken>()))
            .Callback<Domain.Entities.User, CancellationToken>((u, _) => capturedUser = u)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedUser!.ManagerId.Should().Be(currentUserId);
    }

    [Fact]
    public async Task Should_TrimAndLowercaseEmail_When_UserCreated()
    {
        // Arrange
        var managerId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var dto = BuildDto(managerId);
        var command = new CreateUserCommand(dto, currentUserId);

        SetupEmailExists(dto.Email, false);
        SetupManagerExists(managerId, true);

        Domain.Entities.User? capturedUser = null;
        _userRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Domain.Entities.User>(), It.IsAny<CancellationToken>()))
            .Callback<Domain.Entities.User, CancellationToken>((u, _) => capturedUser = u)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedUser!.Email.Should().Be("alice@example.com");
    }

    [Fact]
    public async Task Should_TrimFullName_When_UserCreated()
    {
        // Arrange
        var managerId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var dto = BuildDto(managerId);
        var command = new CreateUserCommand(dto, currentUserId);

        SetupEmailExists(dto.Email, false);
        SetupManagerExists(managerId, true);

        Domain.Entities.User? capturedUser = null;
        _userRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Domain.Entities.User>(), It.IsAny<CancellationToken>()))
            .Callback<Domain.Entities.User, CancellationToken>((u, _) => capturedUser = u)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedUser!.FullName.Should().Be("Alice Johnson");
    }

    [Fact]
    public async Task Should_SetIsActiveTrue_When_UserCreated()
    {
        // Arrange
        var managerId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var dto = BuildDto(managerId);
        var command = new CreateUserCommand(dto, currentUserId);

        SetupEmailExists(dto.Email, false);
        SetupManagerExists(managerId, true);

        Domain.Entities.User? capturedUser = null;
        _userRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Domain.Entities.User>(), It.IsAny<CancellationToken>()))
            .Callback<Domain.Entities.User, CancellationToken>((u, _) => capturedUser = u)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedUser!.IsActive.Should().BeTrue();
    }

    // ─── Email conflict ───────────────────────────────────────────────────────

    [Fact]
    public async Task Should_ThrowInvalidOperationException_When_EmailAlreadyExists()
    {
        // Arrange
        var dto = BuildDto();
        var command = new CreateUserCommand(dto, Guid.NewGuid());

        SetupEmailExists(dto.Email, true);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task Should_NeverCallCreateAsync_When_EmailAlreadyExists()
    {
        // Arrange
        var dto = BuildDto();
        var command = new CreateUserCommand(dto, Guid.NewGuid());

        SetupEmailExists(dto.Email, true);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();

        // Assert
        _userRepositoryMock.Verify(
            r => r.CreateAsync(It.IsAny<Domain.Entities.User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ─── Manager not found ───────────────────────────────────────────────────

    [Fact]
    public async Task Should_ThrowKeyNotFoundException_When_ManagerNotFound()
    {
        // Arrange
        var managerId = Guid.NewGuid();
        var dto = BuildDto(managerId);
        var command = new CreateUserCommand(dto, Guid.NewGuid());

        SetupEmailExists(dto.Email, false);
        SetupManagerExists(managerId, false);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Manager*");
    }

    [Fact]
    public async Task Should_NeverCallCreateAsync_When_ManagerNotFound()
    {
        // Arrange
        var managerId = Guid.NewGuid();
        var dto = BuildDto(managerId);
        var command = new CreateUserCommand(dto, Guid.NewGuid());

        SetupEmailExists(dto.Email, false);
        SetupManagerExists(managerId, false);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();

        // Assert
        _userRepositoryMock.Verify(
            r => r.CreateAsync(It.IsAny<Domain.Entities.User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ─── Repository call verification ────────────────────────────────────────

    [Fact]
    public async Task Should_CallEmailExistsAsync_Once_When_Handled()
    {
        // Arrange
        var managerId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var dto = BuildDto(managerId);
        var command = new CreateUserCommand(dto, currentUserId);

        SetupEmailExists(dto.Email, false);
        SetupManagerExists(managerId, true);
        SetupCreateAsync();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(
            r => r.EmailExistsAsync(dto.Email, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_CallExistsAsync_Once_When_ManagerIdProvided()
    {
        // Arrange
        var managerId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var dto = BuildDto(managerId);
        var command = new CreateUserCommand(dto, currentUserId);

        SetupEmailExists(dto.Email, false);
        SetupManagerExists(managerId, true);
        SetupCreateAsync();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(
            r => r.ExistsAsync(managerId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_NeverCallExistsAsync_When_ManagerIdIsNull()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var dto = BuildDto(managerId: null);
        var command = new CreateUserCommand(dto, currentUserId);

        SetupEmailExists(dto.Email, false);
        SetupCreateAsync();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(
            r => r.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Should_CallCreateAsync_Once_When_UserIsValid()
    {
        // Arrange
        var managerId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var dto = BuildDto(managerId);
        var command = new CreateUserCommand(dto, currentUserId);

        SetupEmailExists(dto.Email, false);
        SetupManagerExists(managerId, true);
        SetupCreateAsync();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(
            r => r.CreateAsync(It.IsAny<Domain.Entities.User>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}