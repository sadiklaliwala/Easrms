using Easrms.Application.DTOs.Lookup;
using Easrms.Application.Features.Lookup.Queries;
using Easrms.Application.Interfaces.Repositories;
using FluentAssertions;
using Moq;

namespace Easrms.Test.features.Lookup.Queries;

public sealed class GetManagersQueryHandlerTests
{
    private readonly Mock<ILookupRepository> _lookupRepositoryMock;
    private readonly GetManagersQueryHandler _handler;

    public GetManagersQueryHandlerTests()
    {
        _lookupRepositoryMock = new Mock<ILookupRepository>();
        _handler = new GetManagersQueryHandler(_lookupRepositoryMock.Object);
    }

    private void SetupGetActiveManagers(IReadOnlyList<ManagerLookupDto> result)
    {
        _lookupRepositoryMock
            .Setup(r => r.GetActiveManagersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
    }

    private static IReadOnlyList<ManagerLookupDto> BuildManagerList() =>
        [
            new() { UserId = Guid.NewGuid(), FullName = "Alice Johnson" },
            new() { UserId = Guid.NewGuid(), FullName = "Bob Smith"    }
        ];

    // ─── Success ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Should_ReturnManagerList_When_ActiveManagersExist()
    {
        // Arrange
        var expected = BuildManagerList();
        SetupGetActiveManagers(expected);

        // Act
        var result = await _handler.Handle(new GetManagersQuery(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Should_ReturnEmptyList_When_NoActiveManagersExist()
    {
        // Arrange
        SetupGetActiveManagers(new List<ManagerLookupDto>());

        // Act
        var result = await _handler.Handle(new GetManagersQuery(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_ReturnCorrectManagerCount_When_ActiveManagersExist()
    {
        // Arrange
        SetupGetActiveManagers(BuildManagerList());

        // Act
        var result = await _handler.Handle(new GetManagersQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
    }

    // ─── Repository call verification ────────────────────────────────────────

    [Fact]
    public async Task Should_CallGetActiveManagersAsync_Once_When_Handled()
    {
        // Arrange
        SetupGetActiveManagers(BuildManagerList());

        // Act
        await _handler.Handle(new GetManagersQuery(), CancellationToken.None);

        // Assert
        _lookupRepositoryMock.Verify(
            r => r.GetActiveManagersAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_NeverCallGetActiveManagersAsync_MoreThanOnce_When_Handled()
    {
        // Arrange
        SetupGetActiveManagers(BuildManagerList());

        // Act
        await _handler.Handle(new GetManagersQuery(), CancellationToken.None);

        // Assert
        _lookupRepositoryMock.Verify(
            r => r.GetActiveManagersAsync(It.IsAny<CancellationToken>()),
            Times.AtMostOnce);
    }
}