// FileName: GetStatusHistoryQueryHandlerTests.cs

using Easrms.Application.DTOs.Comment;
using Easrms.Application.Features.History.Queries;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Common.Enums;
using FluentAssertions;
using Moq;

namespace Easrms.Test.features.History.Queries;

public sealed class GetStatusHistoryQueryHandlerTests
{
    private readonly Mock<ICommentRepository> _commentRepositoryMock;
    private readonly Mock<IRequestRepository> _requestRepositoryMock;
    private readonly GetStatusHistoryQueryHandler _handler;

    public GetStatusHistoryQueryHandlerTests()
    {
        _commentRepositoryMock = new Mock<ICommentRepository>();
        _requestRepositoryMock = new Mock<IRequestRepository>();
        _handler = new GetStatusHistoryQueryHandler(
            _commentRepositoryMock.Object,
            _requestRepositoryMock.Object);
    }

    private void SetupRequestExists(Guid requestId, bool exists)
    {
        _requestRepositoryMock
            .Setup(r => r.ExistsAsync(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
    }

    private void SetupGetStatusHistory(Guid requestId, IReadOnlyList<StatusHistoryDto> result)
    {
        _commentRepositoryMock
            .Setup(r => r.GetStatusHistoryByRequestIdAsync(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
    }

    private static IReadOnlyList<StatusHistoryDto> BuildHistoryList() =>
        [
            new() { HistoryId = Guid.NewGuid(), OldStatus = RequestStatusEnum.Open, NewStatus = RequestStatusEnum.InProgress, ChangedByName = "Alice", ChangedOn = DateTime.UtcNow.AddHours(-2) },
            new() { HistoryId = Guid.NewGuid(), OldStatus = RequestStatusEnum.InProgress, NewStatus = RequestStatusEnum.Approved, ChangedByName = "Bob",   ChangedOn = DateTime.UtcNow.AddHours(-1) }
        ];

    // ─── Success ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Should_ReturnStatusHistoryList_When_RequestExists()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var expected = BuildHistoryList();
        var query = new GetStatusHistoryQuery { RequestId = requestId };

        SetupRequestExists(requestId, true);
        SetupGetStatusHistory(requestId, expected);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Should_ReturnEmptyList_When_RequestExistsButHasNoHistory()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var query = new GetStatusHistoryQuery { RequestId = requestId };

        SetupRequestExists(requestId, true);
        SetupGetStatusHistory(requestId, new List<StatusHistoryDto>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_ReturnCorrectHistoryCount_When_RequestExists()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var expected = BuildHistoryList();
        var query = new GetStatusHistoryQuery { RequestId = requestId };

        SetupRequestExists(requestId, true);
        SetupGetStatusHistory(requestId, expected);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
    }

    // ─── NotFound ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Should_ThrowKeyNotFoundException_When_RequestNotFound()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var query = new GetStatusHistoryQuery { RequestId = requestId };

        SetupRequestExists(requestId, false);

        // Act
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{requestId}*");
    }

    // ─── Repository call verification ────────────────────────────────────────

    [Fact]
    public async Task Should_CallExistsAsync_Once_WithCorrectRequestId_When_Handled()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var query = new GetStatusHistoryQuery { RequestId = requestId };

        SetupRequestExists(requestId, true);
        SetupGetStatusHistory(requestId, BuildHistoryList());

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _requestRepositoryMock.Verify(
            r => r.ExistsAsync(requestId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_CallGetStatusHistoryByRequestIdAsync_Once_When_RequestExists()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var query = new GetStatusHistoryQuery { RequestId = requestId };

        SetupRequestExists(requestId, true);
        SetupGetStatusHistory(requestId, BuildHistoryList());

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _commentRepositoryMock.Verify(
            r => r.GetStatusHistoryByRequestIdAsync(requestId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_NeverCallGetStatusHistoryByRequestIdAsync_When_RequestNotFound()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var query = new GetStatusHistoryQuery { RequestId = requestId };

        SetupRequestExists(requestId, false);

        // Act
        var act = async () => await _handler.Handle(query, CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();

        // Assert
        _commentRepositoryMock.Verify(
            r => r.GetStatusHistoryByRequestIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}