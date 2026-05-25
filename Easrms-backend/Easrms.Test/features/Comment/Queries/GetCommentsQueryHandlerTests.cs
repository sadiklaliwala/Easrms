// FileName: GetCommentsQueryHandlerTests.cs

using Easrms.Application.DTOs.Comment;
using Easrms.Application.Features.Comment.Queries;
using Easrms.Application.Interfaces.Repositories;
using FluentAssertions;
using Moq;

namespace Easrms.Test.features.Comment.Queries;

public sealed class GetCommentsQueryHandlerTests
{
    private readonly Mock<ICommentRepository> _commentRepositoryMock;
    private readonly Mock<IRequestRepository> _requestRepositoryMock;
    private readonly GetCommentsQueryHandler _handler;

    public GetCommentsQueryHandlerTests()
    {
        _commentRepositoryMock = new Mock<ICommentRepository>();
        _requestRepositoryMock = new Mock<IRequestRepository>();
        _handler = new GetCommentsQueryHandler(
            _commentRepositoryMock.Object,
            _requestRepositoryMock.Object);
    }

    private void SetupRequestExists(Guid requestId, bool exists)
    {
        _requestRepositoryMock
            .Setup(r => r.ExistsAsync(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
    }

    private void SetupGetComments(Guid requestId, IReadOnlyList<CommentListDto> result)
    {
        _commentRepositoryMock
            .Setup(r => r.GetCommentsByRequestIdAsync(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
    }

    private static IReadOnlyList<CommentListDto> BuildCommentList() =>
        [
            new() { CommentId = Guid.NewGuid(), CommentText = "First comment",  CommentByName = "Alice", CreatedOn = DateTime.UtcNow },
            new() { CommentId = Guid.NewGuid(), CommentText = "Second comment", CommentByName = "Bob",   CreatedOn = DateTime.UtcNow }
        ];

    // ─── Success ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Should_ReturnCommentList_When_RequestExists()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var expected = BuildCommentList();
        var query = new GetCommentsQuery { RequestId = requestId };

        SetupRequestExists(requestId, true);
        SetupGetComments(requestId, expected);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Should_ReturnEmptyList_When_RequestExistsButHasNoComments()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var query = new GetCommentsQuery { RequestId = requestId };

        SetupRequestExists(requestId, true);
        SetupGetComments(requestId, new List<CommentListDto>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_ReturnCorrectCommentCount_When_RequestExists()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var expected = BuildCommentList();
        var query = new GetCommentsQuery { RequestId = requestId };

        SetupRequestExists(requestId, true);
        SetupGetComments(requestId, expected);

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
        var query = new GetCommentsQuery { RequestId = requestId };

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
        var query = new GetCommentsQuery { RequestId = requestId };

        SetupRequestExists(requestId, true);
        SetupGetComments(requestId, BuildCommentList());

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _requestRepositoryMock.Verify(
            r => r.ExistsAsync(requestId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_CallGetCommentsByRequestIdAsync_Once_When_RequestExists()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var query = new GetCommentsQuery { RequestId = requestId };

        SetupRequestExists(requestId, true);
        SetupGetComments(requestId, BuildCommentList());

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _commentRepositoryMock.Verify(
            r => r.GetCommentsByRequestIdAsync(requestId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_NeverCallGetCommentsByRequestIdAsync_When_RequestNotFound()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var query = new GetCommentsQuery { RequestId = requestId };

        SetupRequestExists(requestId, false);

        // Act
        var act = async () => await _handler.Handle(query, CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();

        // Assert
        _commentRepositoryMock.Verify(
            r => r.GetCommentsByRequestIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}