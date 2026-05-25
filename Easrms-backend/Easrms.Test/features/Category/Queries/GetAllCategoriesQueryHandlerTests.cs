// FileName: GetAllCategoriesQueryHandlerTests.cs

using Easrms.Application.DTOs.Category;
using Easrms.Application.Features.Category.Queries;
using Easrms.Application.Interfaces.Repositories;
using FluentAssertions;
using Moq;

namespace Easrms.Test.features.Category.Queries;

public sealed class GetAllCategoriesQueryHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly GetAllCategoriesQueryHandler _handler;

    public GetAllCategoriesQueryHandlerTests()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _handler = new GetAllCategoriesQueryHandler(_categoryRepositoryMock.Object);
    }

    private void SetupGetPagedReturns(CategoryListWithPaginationDto result)
    {
        _categoryRepositoryMock
            .Setup(r => r.GetPagedCategoriesAsync(
                It.IsAny<CategoryQueryParams>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
    }

    private static CategoryListWithPaginationDto BuildPaginatedResult(int totalCount = 2) => new()
    {
        Items = new List<CategoryListDto>
        {
            new() { CategoryId = Guid.NewGuid(), CategoryName = "Hardware", IsActive = true },
            new() { CategoryId = Guid.NewGuid(), CategoryName = "Software", IsActive = false }
        },
        Pagination = new()
        {
            PageNumber = 1,
            PageSize = 10,
            TotalCount = totalCount,
            TotalPages = 1
        }
    };

    // ─── Success ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Should_ReturnCategoryListWithPaginationDto_When_QueryIsHandled()
    {
        // Arrange
        var expected = BuildPaginatedResult();
        SetupGetPagedReturns(expected);

        var query = new GetAllCategoriesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Should_ReturnEmptyList_When_NoCategoriesExist()
    {
        // Arrange
        var empty = new CategoryListWithPaginationDto
        {
            Items = new List<CategoryListDto>(),
            Pagination = new() { PageNumber = 1, PageSize = 10, TotalCount = 0, TotalPages = 0 }
        };
        SetupGetPagedReturns(empty);

        var query = new GetAllCategoriesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.Pagination.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Should_PassCorrectPageNumber_When_QueryIsHandled()
    {
        // Arrange
        SetupGetPagedReturns(BuildPaginatedResult());

        var query = new GetAllCategoriesQuery { PageNumber = 3, PageSize = 5 };

        CategoryQueryParams? captured = null;
        _categoryRepositoryMock
            .Setup(r => r.GetPagedCategoriesAsync(It.IsAny<CategoryQueryParams>(), It.IsAny<CancellationToken>()))
            .Callback<CategoryQueryParams, CancellationToken>((p, _) => captured = p)
            .ReturnsAsync(BuildPaginatedResult());

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        captured.Should().NotBeNull();
        captured!.PageNumber.Should().Be(3);
        captured.PageSize.Should().Be(5);
    }

    [Fact]
    public async Task Should_PassSearchTerm_When_SearchIsProvided()
    {
        // Arrange
        CategoryQueryParams? captured = null;
        _categoryRepositoryMock
            .Setup(r => r.GetPagedCategoriesAsync(It.IsAny<CategoryQueryParams>(), It.IsAny<CancellationToken>()))
            .Callback<CategoryQueryParams, CancellationToken>((p, _) => captured = p)
            .ReturnsAsync(BuildPaginatedResult());

        var query = new GetAllCategoriesQuery { Search = "Hardware" };

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        captured!.SearchTerm.Should().Be("Hardware");
    }

    [Fact]
    public async Task Should_PassIsActiveFilter_When_IsActiveIsProvided()
    {
        // Arrange
        CategoryQueryParams? captured = null;
        _categoryRepositoryMock
            .Setup(r => r.GetPagedCategoriesAsync(It.IsAny<CategoryQueryParams>(), It.IsAny<CancellationToken>()))
            .Callback<CategoryQueryParams, CancellationToken>((p, _) => captured = p)
            .ReturnsAsync(BuildPaginatedResult());

        var query = new GetAllCategoriesQuery { IsActive = true };

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        captured!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Should_PassNullFilters_When_NoFiltersProvided()
    {
        // Arrange
        CategoryQueryParams? captured = null;
        _categoryRepositoryMock
            .Setup(r => r.GetPagedCategoriesAsync(It.IsAny<CategoryQueryParams>(), It.IsAny<CancellationToken>()))
            .Callback<CategoryQueryParams, CancellationToken>((p, _) => captured = p)
            .ReturnsAsync(BuildPaginatedResult());

        var query = new GetAllCategoriesQuery();

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        captured!.SearchTerm.Should().BeNull();
        captured.IsActive.Should().BeNull();
    }

    // ─── Repository call verification ────────────────────────────────────────

    [Fact]
    public async Task Should_CallGetPagedCategoriesAsync_Once_When_QueryIsHandled()
    {
        // Arrange
        SetupGetPagedReturns(BuildPaginatedResult());

        var query = new GetAllCategoriesQuery();

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _categoryRepositoryMock.Verify(
            r => r.GetPagedCategoriesAsync(It.IsAny<CategoryQueryParams>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}