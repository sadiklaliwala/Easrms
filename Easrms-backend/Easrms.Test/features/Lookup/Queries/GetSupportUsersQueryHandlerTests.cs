using Easrms.Application.DTOs.Lookup;
using Easrms.Application.Features.Lookup.Queries;
using Easrms.Application.Interfaces.Repositories;
using FluentAssertions;
using Moq;

namespace Easrms.Test.features.Lookup.Queries;

public sealed class GetSupportUsersQueryHandlerTests
{
    private readonly Mock<ILookupRepository> _lookupRepositoryMock;
    private readonly GetSupportUsersQueryHandler _handler;

    public GetSupportUsersQueryHandlerTests()
    {
        _lookupRepositoryMock = new Mock<ILookupRepository>();
        _handler = new GetSupportUsersQueryHandler(_lookupRepositoryMock.Object);
    }

    private void SetupGetActiveSupportUsers(IReadOnlyList<SupportUserLookupDto> result)
    {
        _lookupRepositoryMock
            .Setup(r => r.GetActiveSupportUsersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
    }

    private static IReadOnlyList<SupportUserLookupDto> BuildSupportUserList() =>
        [
            new() { UserId = Guid.NewGuid(), FullName = "Carol White" },
            new() { UserId = Guid.NewGuid(), FullName = "Dave Brown" }
        ];

    // ─── Success ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Should_ReturnSupportUserList_When_ActiveSupportUsersExist()
    {
        // Arrange
        var expected = BuildSupportUserList();
        SetupGetActiveSupportUsers(expected);

        // Act
        var result = await _handler.Handle(new GetSupportUsersQuery(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Should_ReturnEmptyList_When_NoActiveSupportUsersExist()
    {
        // Arrange
        SetupGetActiveSupportUsers(new List<SupportUserLookupDto>());

        // Act
        var result = await _handler.Handle(new GetSupportUsersQuery(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_ReturnCorrectSupportUserCount_When_ActiveSupportUsersExist()
    {
        // Arrange
        SetupGetActiveSupportUsers(BuildSupportUserList());

        // Act
        var result = await _handler.Handle(new GetSupportUsersQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
    }

    // ─── Repository call verification ────────────────────────────────────────

    [Fact]
    public async Task Should_CallGetActiveSupportUsersAsync_Once_When_Handled()
    {
        // Arrange
        SetupGetActiveSupportUsers(BuildSupportUserList());

        // Act
        await _handler.Handle(new GetSupportUsersQuery(), CancellationToken.None);

        // Assert
        _lookupRepositoryMock.Verify(
            r => r.GetActiveSupportUsersAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_NeverCallGetActiveSupportUsersAsync_MoreThanOnce_When_Handled()
    {
        // Arrange
        SetupGetActiveSupportUsers(BuildSupportUserList());

        // Act
        await _handler.Handle(new GetSupportUsersQuery(), CancellationToken.None);

        // Assert
        _lookupRepositoryMock.Verify(
            r => r.GetActiveSupportUsersAsync(It.IsAny<CancellationToken>()),
            Times.AtMostOnce);
    }
}