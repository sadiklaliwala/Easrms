// FileName: UpdateCategoryCommandHandlerTests.cs

using Easrms.Application.Features.Category.Commands;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Domain.Entities;
using FluentAssertions;
using Moq;

namespace Easrms.Test.features.Category.Commands;

public sealed class UpdateCategoryCommandHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly UpdateCategoryCommandHandler _handler;

    public UpdateCategoryCommandHandlerTests()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _handler = new UpdateCategoryCommandHandler(_categoryRepositoryMock.Object);
    }

    private void SetupGetByIdReturns(Guid categoryId, RequestCategory? category)
    {
        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
    }

    private void SetupNameTaken(Guid excludeId, bool taken)
    {
        _categoryRepositoryMock
            .Setup(r => r.IsCategoryNameTakenAsync(
                It.IsAny<string>(),
                excludeId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(taken);
    }

    private void SetupSaveChanges()
    {
        _categoryRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }

    private static RequestCategory CreateCategory(Guid? id = null) => new()
    {
        CategoryId = id ?? Guid.NewGuid(),
        CategoryName = "OldName",
        IsApprovalRequired = false,
        IsActive = true,
        CreatedOn = DateTime.UtcNow.AddDays(-5)
    };

    private static UpdateCategoryCommand BuildCommand(
        Guid categoryId,
        string name = "NewName",
        bool isApprovalRequired = false,
        int slaHours = 24) => new()
        {
            CategoryId = categoryId,
            CategoryName = name,
            IsApprovalRequired = isApprovalRequired,
            SLAHours = slaHours
        };

    // ─── Success ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Should_UpdateCategoryProperties_When_CommandIsValid()
    {
        // Arrange
        var category = CreateCategory();
        var command = BuildCommand(category.CategoryId, name: "UpdatedName", isApprovalRequired: true, slaHours: 48);

        SetupGetByIdReturns(category.CategoryId, category);
        SetupNameTaken(category.CategoryId, false);
        SetupSaveChanges();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        category.CategoryName.Should().Be("UpdatedName");
        category.IsApprovalRequired.Should().BeTrue();
        category.SLAHours.Should().Be(48);
    }

    [Fact]
    public async Task Should_SetUpdatedOn_When_CommandIsValid()
    {
        // Arrange
        var category = CreateCategory();
        var command = BuildCommand(category.CategoryId);

        SetupGetByIdReturns(category.CategoryId, category);
        SetupNameTaken(category.CategoryId, false);
        SetupSaveChanges();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        category.UpdatedOn.Should().NotBeNull();
        category.UpdatedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Should_CompleteWithoutException_When_CommandIsValid()
    {
        // Arrange
        var category = CreateCategory();
        var command = BuildCommand(category.CategoryId);

        SetupGetByIdReturns(category.CategoryId, category);
        SetupNameTaken(category.CategoryId, false);
        SetupSaveChanges();

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    // ─── NotFound ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Should_ThrowKeyNotFoundException_When_CategoryNotFound()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = BuildCommand(categoryId);

        SetupGetByIdReturns(categoryId, null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{categoryId}*");
    }

    // ─── Conflict ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Should_ThrowInvalidOperationException_When_CategoryNameAlreadyTaken()
    {
        // Arrange
        var category = CreateCategory();
        var command = BuildCommand(category.CategoryId, name: "DuplicateName");

        SetupGetByIdReturns(category.CategoryId, category);
        SetupNameTaken(category.CategoryId, true);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("*DuplicateName*already exists*");
    }

    // ─── SLA Validation ───────────────────────────────────────────────────────

    [Fact]
    public async Task Should_ThrowInvalidOperationException_When_SLAHoursIsZero()
    {
        // Arrange
        var category = CreateCategory();
        var command = BuildCommand(category.CategoryId, slaHours: 0);

        SetupGetByIdReturns(category.CategoryId, category);
        SetupNameTaken(category.CategoryId, false);

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
        var category = CreateCategory();
        var command = BuildCommand(category.CategoryId, slaHours: -10);

        SetupGetByIdReturns(category.CategoryId, category);
        SetupNameTaken(category.CategoryId, false);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("SLA hours must be positive.");
    }

    // ─── Repository call verification ────────────────────────────────────────

    [Fact]
    public async Task Should_CallGetByIdAsync_Once_When_Handled()
    {
        // Arrange
        var category = CreateCategory();
        var command = BuildCommand(category.CategoryId);

        SetupGetByIdReturns(category.CategoryId, category);
        SetupNameTaken(category.CategoryId, false);
        SetupSaveChanges();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _categoryRepositoryMock.Verify(
            r => r.GetByIdAsync(category.CategoryId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_CallIsCategoryNameTakenAsync_WithExcludeSelfId_When_Handled()
    {
        // Arrange
        var category = CreateCategory();
        var command = BuildCommand(category.CategoryId, name: "NewName");

        SetupGetByIdReturns(category.CategoryId, category);
        SetupNameTaken(category.CategoryId, false);
        SetupSaveChanges();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _categoryRepositoryMock.Verify(
            r => r.IsCategoryNameTakenAsync("NewName", category.CategoryId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_CallUpdate_Once_When_CommandIsValid()
    {
        // Arrange
        var category = CreateCategory();
        var command = BuildCommand(category.CategoryId);

        SetupGetByIdReturns(category.CategoryId, category);
        SetupNameTaken(category.CategoryId, false);
        SetupSaveChanges();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _categoryRepositoryMock.Verify(
            r => r.Update(category),
            Times.Once);
    }

    [Fact]
    public async Task Should_CallSaveChangesAsync_Once_When_CommandIsValid()
    {
        // Arrange
        var category = CreateCategory();
        var command = BuildCommand(category.CategoryId);

        SetupGetByIdReturns(category.CategoryId, category);
        SetupNameTaken(category.CategoryId, false);
        SetupSaveChanges();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _categoryRepositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_NeverCallUpdate_When_CategoryNotFound()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = BuildCommand(categoryId);

        SetupGetByIdReturns(categoryId, null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();

        // Assert
        _categoryRepositoryMock.Verify(
            r => r.Update(It.IsAny<RequestCategory>()),
            Times.Never);
    }

    [Fact]
    public async Task Should_NeverCallSaveChangesAsync_When_CategoryNotFound()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = BuildCommand(categoryId);

        SetupGetByIdReturns(categoryId, null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();

        // Assert
        _categoryRepositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Should_NeverCallUpdate_When_CategoryNameAlreadyTaken()
    {
        // Arrange
        var category = CreateCategory();
        var command = BuildCommand(category.CategoryId, name: "DuplicateName");

        SetupGetByIdReturns(category.CategoryId, category);
        SetupNameTaken(category.CategoryId, true);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();

        // Assert
        _categoryRepositoryMock.Verify(
            r => r.Update(It.IsAny<RequestCategory>()),
            Times.Never);
    }

    [Fact]
    public async Task Should_NeverCallSaveChangesAsync_When_CategoryNameAlreadyTaken()
    {
        // Arrange
        var category = CreateCategory();
        var command = BuildCommand(category.CategoryId, name: "DuplicateName");

        SetupGetByIdReturns(category.CategoryId, category);
        SetupNameTaken(category.CategoryId, true);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();

        // Assert
        _categoryRepositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Should_NeverCallUpdate_When_SLAHoursIsInvalid()
    {
        // Arrange
        var category = CreateCategory();
        var command = BuildCommand(category.CategoryId, slaHours: 0);

        SetupGetByIdReturns(category.CategoryId, category);
        SetupNameTaken(category.CategoryId, false);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();

        // Assert
        _categoryRepositoryMock.Verify(
            r => r.Update(It.IsAny<RequestCategory>()),
            Times.Never);
    }

    [Fact]
    public async Task Should_NeverCallSaveChangesAsync_When_SLAHoursIsInvalid()
    {
        // Arrange
        var category = CreateCategory();
        var command = BuildCommand(category.CategoryId, slaHours: -1);

        SetupGetByIdReturns(category.CategoryId, category);
        SetupNameTaken(category.CategoryId, false);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();

        // Assert
        _categoryRepositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}