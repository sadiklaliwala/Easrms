// FileName: CreateCategoryCommandHandlerTests.cs

using Easrms.Application.Features.Category.Commands;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Domain.Entities;
using FluentAssertions;
using Moq;

namespace Easrms.Test.features.Category.Commands;

public sealed class CreateCategoryCommandHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly CreateCategoryCommandHandler _handler;

    public CreateCategoryCommandHandlerTests()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _handler = new CreateCategoryCommandHandler(_categoryRepositoryMock.Object);
    }

    private void SetupNameTaken(bool taken)
    {
        _categoryRepositoryMock
            .Setup(r => r.IsCategoryNameTakenAsync(
                It.IsAny<string>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(taken);
    }

    private static CreateCategoryCommand BuildCommand(
        string name = "Hardware",
        bool isApprovalRequired = false,
        int slaHours = 24) => new()
        {
            CategoryName = name,
            IsApprovalRequired = isApprovalRequired,
            SLAHours = slaHours
        };

    // ─── Success ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Should_ReturnNewCategoryId_When_CommandIsValid()
    {
        // Arrange
        SetupNameTaken(false);
        _categoryRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<RequestCategory>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _categoryRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = BuildCommand();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Should_SetIsActiveTrue_When_CategoryIsCreated()
    {
        // Arrange
        SetupNameTaken(false);
        RequestCategory? captured = null;
        _categoryRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<RequestCategory>(), It.IsAny<CancellationToken>()))
            .Callback<RequestCategory, CancellationToken>((c, _) => captured = c)
            .Returns(Task.CompletedTask);
        _categoryRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = BuildCommand();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        captured.Should().NotBeNull();
        captured!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Should_SetCorrectProperties_When_CategoryIsCreated()
    {
        // Arrange
        SetupNameTaken(false);
        RequestCategory? captured = null;
        _categoryRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<RequestCategory>(), It.IsAny<CancellationToken>()))
            .Callback<RequestCategory, CancellationToken>((c, _) => captured = c)
            .Returns(Task.CompletedTask);
        _categoryRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = BuildCommand(name: "Software", isApprovalRequired: true, slaHours: 48);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        captured!.CategoryName.Should().Be("Software");
        captured.IsApprovalRequired.Should().BeTrue();
        captured.SLAHours.Should().Be(48);
        captured.CreatedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    // ─── Conflict ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Should_ThrowInvalidOperationException_When_CategoryNameAlreadyExists()
    {
        // Arrange
        SetupNameTaken(true);
        var command = BuildCommand(name: "Hardware");

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("*Hardware*already exists*");
    }

    // ─── SLA Validation ───────────────────────────────────────────────────────

    [Fact]
    public async Task Should_ThrowInvalidOperationException_When_SLAHoursIsZero()
    {
        // Arrange
        SetupNameTaken(false);
        var command = BuildCommand(slaHours: 0);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("SLA hours must be positive.");
    }

    [Fact]
    public async Task Should_ThrowInvalidOperationException_When_SLAHoursIsNegative()
    {
        // Arrange
        SetupNameTaken(false);
        var command = BuildCommand(slaHours: -5);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("SLA hours must be positive.");
    }

    // ─── Repository call verification ────────────────────────────────────────

    [Fact]
    public async Task Should_CallIsCategoryNameTakenAsync_WithNullExcludeId_When_Handled()
    {
        // Arrange
        SetupNameTaken(false);
        _categoryRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<RequestCategory>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _categoryRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = BuildCommand();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _categoryRepositoryMock.Verify(
            r => r.IsCategoryNameTakenAsync(command.CategoryName, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_CallAddAsync_Once_When_CommandIsValid()
    {
        // Arrange
        SetupNameTaken(false);
        _categoryRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<RequestCategory>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _categoryRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = BuildCommand();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _categoryRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<RequestCategory>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_CallSaveChangesAsync_Once_When_CommandIsValid()
    {
        // Arrange
        SetupNameTaken(false);
        _categoryRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<RequestCategory>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _categoryRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = BuildCommand();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _categoryRepositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_NeverCallAddAsync_When_CategoryNameAlreadyExists()
    {
        // Arrange
        SetupNameTaken(true);
        var command = BuildCommand();

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();

        // Assert
        _categoryRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<RequestCategory>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Should_NeverCallSaveChangesAsync_When_CategoryNameAlreadyExists()
    {
        // Arrange
        SetupNameTaken(true);
        var command = BuildCommand();

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();

        // Assert
        _categoryRepositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Should_NeverCallAddAsync_When_SLAHoursIsInvalid()
    {
        // Arrange
        SetupNameTaken(false);
        var command = BuildCommand(slaHours: 0);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();

        // Assert
        _categoryRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<RequestCategory>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Should_NeverCallSaveChangesAsync_When_SLAHoursIsInvalid()
    {
        // Arrange
        SetupNameTaken(false);
        var command = BuildCommand(slaHours: -1);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();

        // Assert
        _categoryRepositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}