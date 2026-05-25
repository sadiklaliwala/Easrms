// FileName: AddCommentCommandHandlerTests.cs

using Easrms.Application.Features.Comment.Commands;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Domain.Entities;
using Easrms.Common.Enums;
using FluentAssertions;
using Moq;

namespace Easrms.Test.features.Comment.Commands;

public sealed class AddCommentCommandHandlerTests
{
    private readonly Mock<ICommentRepository> _commentRepositoryMock;
    private readonly Mock<IRequestRepository> _requestRepositoryMock;
    private readonly AddCommentCommandHandler _handler;

    public AddCommentCommandHandlerTests()
    {
        _commentRepositoryMock = new Mock<ICommentRepository>();
        _requestRepositoryMock = new Mock<IRequestRepository>();
        _handler = new AddCommentCommandHandler(
            _commentRepositoryMock.Object,
            _requestRepositoryMock.Object);
    }

    private void SetupRequestExists(Guid requestId, bool exists)
    {
        _requestRepositoryMock
            .Setup(r => r.ExistsAsync(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
    }

    private void SetupAddComment()
    {
        _commentRepositoryMock
            .Setup(r => r.AddCommentAsync(It.IsAny<RequestComment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private void SetupSaveChanges()
    {
        _commentRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }

    private static AddCommentCommand BuildCommand(
        Guid? requestId = null,
        Guid? commentBy = null,
        string text = "Test comment",
        int commentType = 1) => new()
        {
            RequestId = requestId ?? Guid.NewGuid(),
            CommentBy = commentBy ?? Guid.NewGuid(),
            CommentText = text,
            CommentType = commentType
        };

    // ─── Success ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Should_ReturnNewCommentId_When_CommandIsValid()
    {
        // Arrange
        var command = BuildCommand();

        SetupRequestExists(command.RequestId, true);
        SetupAddComment();
        SetupSaveChanges();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Should_SetCorrectProperties_When_CommentIsCreated()
    {
        // Arrange
        var command = BuildCommand(text: "My comment", commentType: 1);
        RequestComment? captured = null;

        SetupRequestExists(command.RequestId, true);
        _commentRepositoryMock
            .Setup(r => r.AddCommentAsync(It.IsAny<RequestComment>(), It.IsAny<CancellationToken>()))
            .Callback<RequestComment, CancellationToken>((c, _) => captured = c)
            .Returns(Task.CompletedTask);
        SetupSaveChanges();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        captured.Should().NotBeNull();
        captured!.RequestId.Should().Be(command.RequestId);
        captured.CommentBy.Should().Be(command.CommentBy);
        captured.CommentText.Should().Be("My comment");
        captured.CommentType.Should().Be((CommentTypeEnum)1);
        captured.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task Should_SetCreatedOnToUtcNow_When_CommentIsCreated()
    {
        // Arrange
        var command = BuildCommand();
        RequestComment? captured = null;

        SetupRequestExists(command.RequestId, true);
        _commentRepositoryMock
            .Setup(r => r.AddCommentAsync(It.IsAny<RequestComment>(), It.IsAny<CancellationToken>()))
            .Callback<RequestComment, CancellationToken>((c, _) => captured = c)
            .Returns(Task.CompletedTask);
        SetupSaveChanges();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        captured!.CreatedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Should_SetIsDeletedFalse_When_CommentIsCreated()
    {
        // Arrange
        var command = BuildCommand();
        RequestComment? captured = null;

        SetupRequestExists(command.RequestId, true);
        _commentRepositoryMock
            .Setup(r => r.AddCommentAsync(It.IsAny<RequestComment>(), It.IsAny<CancellationToken>()))
            .Callback<RequestComment, CancellationToken>((c, _) => captured = c)
            .Returns(Task.CompletedTask);
        SetupSaveChanges();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        captured!.IsDeleted.Should().BeFalse();
    }

    // ─── NotFound ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Should_ThrowKeyNotFoundException_When_RequestNotFound()
    {
        // Arrange
        var command = BuildCommand();

        SetupRequestExists(command.RequestId, false);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{command.RequestId}*");
    }

    // ─── Repository call verification ────────────────────────────────────────

    [Fact]
    public async Task Should_CallExistsAsync_Once_WithCorrectRequestId_When_Handled()
    {
        // Arrange
        var command = BuildCommand();

        SetupRequestExists(command.RequestId, true);
        SetupAddComment();
        SetupSaveChanges();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _requestRepositoryMock.Verify(
            r => r.ExistsAsync(command.RequestId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_CallAddCommentAsync_Once_When_CommandIsValid()
    {
        // Arrange
        var command = BuildCommand();

        SetupRequestExists(command.RequestId, true);
        SetupAddComment();
        SetupSaveChanges();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _commentRepositoryMock.Verify(
            r => r.AddCommentAsync(It.IsAny<RequestComment>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_CallSaveChangesAsync_Once_When_CommandIsValid()
    {
        // Arrange
        var command = BuildCommand();

        SetupRequestExists(command.RequestId, true);
        SetupAddComment();
        SetupSaveChanges();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _commentRepositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_NeverCallAddCommentAsync_When_RequestNotFound()
    {
        // Arrange
        var command = BuildCommand();

        SetupRequestExists(command.RequestId, false);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();

        // Assert
        _commentRepositoryMock.Verify(
            r => r.AddCommentAsync(It.IsAny<RequestComment>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Should_NeverCallSaveChangesAsync_When_RequestNotFound()
    {
        // Arrange
        var command = BuildCommand();

        SetupRequestExists(command.RequestId, false);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();

        // Assert
        _commentRepositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}