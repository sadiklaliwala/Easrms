using Easrms.Application.DTOs.Common;
using Easrms.Application.DTOs.User;
using Easrms.Application.Features.User.Queries;
using Easrms.Application.Interfaces.Repositories;
using FluentAssertions;
using Moq;

namespace Easrms.Test.features.Users.Queries;

public sealed class GetAllUsersQueryHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly GetAllUsersQueryHandler _handler;

    public GetAllUsersQueryHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _handler = new GetAllUsersQueryHandler(_userRepositoryMock.Object);
    }

    private void SetupGetAllAsync(UserListWithPaginationDto result)
    {
        _userRepositoryMock
            .Setup(r => r.GetAllAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string?>(),
                It.IsAny<Guid?>(),
                It.IsAny<bool?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
    }

    private static UserQueryParams BuildQueryParams(
        int pageNumber = 1,
        int pageSize = 10,
        string? search = null,
        Guid? roleId = null,
        bool? isActive = null,
        string? sortBy = null,
        string? sortDir = null) => new()
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Search = search,
            RoleId = roleId,
            IsActive = isActive,
            SortBy = sortBy,
            SortDirection = sortDir
        };

    private static UserListWithPaginationDto BuildPaginatedResult(int totalCount = 2) => new()
    {
        Items = new List<UserListDto>
        {
            new() { UserId = Guid.NewGuid(), FullName = "Alice Johnson", Email = "alice@example.com" },
            new() { UserId = Guid.NewGuid(), FullName = "Bob Smith",     Email = "bob@example.com"   }
        },
        Pagination = new PaginationDto
        {
            TotalCount = totalCount,
            PageNumber = 1,
            PageSize = 10
        }
    };

    // ─── Success ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Should_ReturnPaginatedResult_When_UsersExist()
    {
        // Arrange
        var expected = BuildPaginatedResult();
        var query = new GetAllUsersQuery(BuildQueryParams());

        SetupGetAllAsync(expected);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Should_ReturnEmptyItems_When_NoUsersMatch()
    {
        // Arrange
        var empty = new UserListWithPaginationDto
        {
            Items = new List<UserListDto>(),
            Pagination = new PaginationDto
            {
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 0
            }
            
        };
        var query = new GetAllUsersQuery(BuildQueryParams());

        SetupGetAllAsync(empty);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.Pagination.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Should_PassAllQueryParams_ToRepository_When_Handled()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var @params = BuildQueryParams(
            pageNumber: 2,
            pageSize: 5,
            search: "alice",
            roleId: roleId,
            isActive: true,
            sortBy: "FullName",
            sortDir: "asc");

        var query = new GetAllUsersQuery(@params);
        SetupGetAllAsync(BuildPaginatedResult());

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(r => r.GetAllAsync(
            2,
            5,
            "alice",
            roleId,
            true,
            "FullName",
            "asc",
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ─── Repository call verification ────────────────────────────────────────

    [Fact]
    public async Task Should_CallGetAllAsync_Once_When_Handled()
    {
        // Arrange
        var query = new GetAllUsersQuery(BuildQueryParams());
        SetupGetAllAsync(BuildPaginatedResult());

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(r => r.GetAllAsync(
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<string?>(),
            It.IsAny<Guid?>(),
            It.IsAny<bool?>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}