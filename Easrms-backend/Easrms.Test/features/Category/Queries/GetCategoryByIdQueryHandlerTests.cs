// FileName: GetCategoryByIdQueryHandlerTests.cs

using AutoMapper;
using Easrms.Application.DTOs.Category;
using Easrms.Application.Features.Category.Queries;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Domain.Entities;
using FluentAssertions;
using Moq;

namespace Easrms.Test.features.Category.Queries;

public sealed class GetCategoryByIdQueryHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetCategoryByIdQueryHandler _handler;

    public GetCategoryByIdQueryHandlerTests()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _mapperMock = new Mock<IMapper>();
        _handler = new GetCategoryByIdQueryHandler(
            _categoryRepositoryMock.Object,
            _mapperMock.Object);
    }

    private void SetupGetByIdReturns(Guid categoryId, RequestCategory? category)
    {
        _categoryRepositoryMock
            .Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
    }

    private static RequestCategory CreateCategory(Guid? id = null) => new()
    {
        CategoryId = id ?? Guid.NewGuid(),
        CategoryName = "Hardware",
        IsApprovalRequired = false,
        IsActive = true,
        CreatedOn = DateTime.UtcNow.AddDays(-3)
    };

    private static CategoryDetailDto CreateDetailDto(RequestCategory category) => new()
    {
        CategoryId = category.CategoryId,
        CategoryName = category.CategoryName,
        IsApprovalRequired = category.IsApprovalRequired,
        IsActive = category.IsActive,
        CreatedOn = category.CreatedOn
    };

    // ─── Success ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Should_ReturnCategoryDetailDto_When_CategoryExists()
    {
        // Arrange
        var category = CreateCategory();
        var expectedDto = CreateDetailDto(category);
        var query = new GetCategoryByIdQuery { CategoryId = category.CategoryId };

        SetupGetByIdReturns(category.CategoryId, category);
        _mapperMock.Setup(m => m.Map<CategoryDetailDto>(category)).Returns(expectedDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedDto);
    }

    [Fact]
    public async Task Should_ReturnCorrectCategoryId_When_CategoryExists()
    {
        // Arrange
        var category = CreateCategory();
        var expectedDto = CreateDetailDto(category);
        var query = new GetCategoryByIdQuery { CategoryId = category.CategoryId };

        SetupGetByIdReturns(category.CategoryId, category);
        _mapperMock.Setup(m => m.Map<CategoryDetailDto>(category)).Returns(expectedDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.CategoryId.Should().Be(category.CategoryId);
    }

    // ─── NotFound ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Should_ThrowKeyNotFoundException_When_CategoryNotFound()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var query = new GetCategoryByIdQuery { CategoryId = categoryId };

        SetupGetByIdReturns(categoryId, null);

        // Act
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{categoryId}*");
    }

    // ─── Repository call verification ────────────────────────────────────────

    [Fact]
    public async Task Should_CallGetByIdAsync_Once_WithCorrectId_When_Handled()
    {
        // Arrange
        var category = CreateCategory();
        var query = new GetCategoryByIdQuery { CategoryId = category.CategoryId };

        SetupGetByIdReturns(category.CategoryId, category);
        _mapperMock.Setup(m => m.Map<CategoryDetailDto>(category)).Returns(CreateDetailDto(category));

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _categoryRepositoryMock.Verify(
            r => r.GetByIdAsync(category.CategoryId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ─── Mapper call verification ─────────────────────────────────────────────

    [Fact]
    public async Task Should_CallMapper_Once_When_CategoryExists()
    {
        // Arrange
        var category = CreateCategory();
        var query = new GetCategoryByIdQuery { CategoryId = category.CategoryId };

        SetupGetByIdReturns(category.CategoryId, category);
        _mapperMock.Setup(m => m.Map<CategoryDetailDto>(category)).Returns(CreateDetailDto(category));

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mapperMock.Verify(
            m => m.Map<CategoryDetailDto>(category),
            Times.Once);
    }

    [Fact]
    public async Task Should_NeverCallMapper_When_CategoryNotFound()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var query = new GetCategoryByIdQuery { CategoryId = categoryId };

        SetupGetByIdReturns(categoryId, null);

        // Act
        var act = async () => await _handler.Handle(query, CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();

        // Assert
        _mapperMock.Verify(
            m => m.Map<CategoryDetailDto>(It.IsAny<RequestCategory>()),
            Times.Never);
    }
}