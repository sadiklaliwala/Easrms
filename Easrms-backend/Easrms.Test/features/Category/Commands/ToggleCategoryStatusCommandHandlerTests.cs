// FileName: ToggleCategoryStatusCommandHandlerTests.cs

using Easrms.Application.Features.Category.Commands;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Domain.Entities;
using FluentAssertions;
using Moq;

namespace Easrms.Test.features.Category.Commands;

public sealed class ToggleCategoryStatusCommandHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly ToggleCategoryStatusCommandHandler _handler;

    public ToggleCategoryStatusCommandHandlerTests()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _handler = new ToggleCategoryStatusCommandHandler(_categoryRepositoryMock.Object);
    }

    private void SetupGetByIdReturns(Guid categoryId, RequestCategory? category)
    {
        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
    }

    private void SetupSaveChanges()
    {
        _categoryRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }

    private static RequestCategory CreateCategory(bool isActive = true) => new()
    {
        CategoryId = Guid.NewGuid(),
        CategoryName = "Hardware",
        IsApprovalRequired = false,
        IsActive = isActive,
        CreatedOn = DateTime.UtcNow.AddDays(-1)
    };

    // ─── Success ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Should_ReturnFalse_When_ActiveCategoryIsToggled()
    {
        // Arrange
        var category = CreateCategory(isActive: true);
        var command = new ToggleCategoryStatusCommand { CategoryId = category.CategoryId };

        SetupGetByIdReturns(category.CategoryId, category);
        SetupSaveChanges();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Should_ReturnTrue_When_InactiveCategoryIsToggled()
    {
        // Arrange
        var category = CreateCategory(isActive: false);
        var command = new ToggleCategoryStatusCommand { CategoryId = category.CategoryId };

        SetupGetByIdReturns(category.CategoryId, category);
        SetupSaveChanges();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Should_SetUpdatedOn_When_CategoryIsToggled()
    {
        // Arrange
        var category = CreateCategory();
        var command = new ToggleCategoryStatusCommand { CategoryId = category.CategoryId };

        SetupGetByIdReturns(category.CategoryId, category);
        SetupSaveChanges();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        category.UpdatedOn.Should().NotBeNull();
        category.UpdatedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Should_FlipIsActive_When_CategoryIsToggled()
    {
        // Arrange
        var category = CreateCategory(isActive: true);
        var command = new ToggleCategoryStatusCommand { CategoryId = category.CategoryId };

        SetupGetByIdReturns(category.CategoryId, category);
        SetupSaveChanges();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        category.IsActive.Should().BeFalse();
    }

    // ─── NotFound ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Should_ThrowKeyNotFoundException_When_CategoryNotFound()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new ToggleCategoryStatusCommand { CategoryId = categoryId };

        SetupGetByIdReturns(categoryId, null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{categoryId}*");
    }

    // ─── Repository call verification ────────────────────────────────────────

    [Fact]
    public async Task Should_CallGetByIdAsync_Once_When_Handled()
    {
        // Arrange
        var category = CreateCategory();
        var command = new ToggleCategoryStatusCommand { CategoryId = category.CategoryId };

        SetupGetByIdReturns(category.CategoryId, category);
        SetupSaveChanges();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _categoryRepositoryMock.Verify(
            r => r.GetByIdAsync(category.CategoryId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_CallUpdate_Once_When_CategoryFound()
    {
        // Arrange
        var category = CreateCategory();
        var command = new ToggleCategoryStatusCommand { CategoryId = category.CategoryId };

        SetupGetByIdReturns(category.CategoryId, category);
        SetupSaveChanges();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _categoryRepositoryMock.Verify(
            r => r.Update(category),
            Times.Once);
    }

    [Fact]
    public async Task Should_CallSaveChangesAsync_Once_When_CategoryFound()
    {
        // Arrange
        var category = CreateCategory();
        var command = new ToggleCategoryStatusCommand { CategoryId = category.CategoryId };

        SetupGetByIdReturns(category.CategoryId, category);
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
        var command = new ToggleCategoryStatusCommand { CategoryId = categoryId };

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
        var command = new ToggleCategoryStatusCommand { CategoryId = categoryId };

        SetupGetByIdReturns(categoryId, null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();

        // Assert
        _categoryRepositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}