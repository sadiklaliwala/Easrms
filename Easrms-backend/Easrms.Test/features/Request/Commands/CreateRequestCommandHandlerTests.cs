using System;
using System.Threading;
using System.Threading.Tasks;
using Easrms.Application.Features.Request.Commands;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Application.Interfaces;
using Easrms.Common.Enums;
using Easrms.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace Easrms.Test.features.Request.Commands;

public class CreateRequestCommandHandlerTests
{
    private readonly Mock<IRequestRepository> _mockRequestRepository;
    private readonly Mock<ICategoryRepository> _mockCategoryRepository;
    private readonly Mock<ICommentRepository> _mockCommentRepository;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly CreateRequestCommandHandler _handler;

    public CreateRequestCommandHandlerTests()
    {
        _mockRequestRepository = new Mock<IRequestRepository>();
        _mockCategoryRepository = new Mock<ICategoryRepository>();
        _mockCommentRepository = new Mock<ICommentRepository>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockEmailService = new Mock<IEmailService>();

        _handler = new CreateRequestCommandHandler(
            _mockRequestRepository.Object,
            _mockCategoryRepository.Object,
            _mockCommentRepository.Object,
            _mockUserRepository.Object,
            _mockEmailService.Object
        );
    }

    [Fact]
    public async Task Should_CreateRequest_When_CategoryIsValid()
    {
        // Arrange
        var command = new CreateRequestCommand
        {
            CategoryId = Guid.NewGuid(),
            Title = "Test Request",
            Description = "Test Description",
            Priority = PriorityEnums.High,
            CurrentUserId = Guid.NewGuid(),
            AttachmentUrl = "https://example.com/attachment"
        };

        var category = new RequestCategory
        {
            CategoryId = command.CategoryId,
            CategoryName = "Test Category",
            IsActive = true,
            SLAHours = 24,
            IsApprovalRequired = false
        };

        _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(command.CategoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _mockRequestRepository.Setup(repo => repo.IsRequestNumberExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockRequestRepository.Setup(repo => repo.AddAsync(It.IsAny<ServiceRequest>(), It.IsAny<CancellationToken>()))
            .Verifiable();

        _mockCommentRepository.Setup(repo => repo.AddStatusHistoryAsync(It.IsAny<RequestStatusHistory>(), It.IsAny<CancellationToken>()))
            .Verifiable();

        _mockRequestRepository.Setup(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNullOrEmpty();
        _mockRequestRepository.Verify(repo => repo.AddAsync(It.IsAny<ServiceRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockCommentRepository.Verify(repo => repo.AddStatusHistoryAsync(It.IsAny<RequestStatusHistory>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockRequestRepository.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_ThrowKeyNotFoundException_When_CategoryDoesNotExist()
    {
        // Arrange
        var command = new CreateRequestCommand
        {
            CategoryId = Guid.NewGuid(),
            Title = "Test Request",
            Description = "Test Description",
            Priority = PriorityEnums.High,
            CurrentUserId = Guid.NewGuid(),
            AttachmentUrl = "https://example.com/attachment"
        };

        _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(command.CategoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RequestCategory)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Category with id '{command.CategoryId}' was not found.");
    }

    [Fact]
    public async Task Should_ThrowInvalidOperationException_When_CategoryIsInactive()
    {
        // Arrange
        var command = new CreateRequestCommand
        {
            CategoryId = Guid.NewGuid(),
            Title = "Test Request",
            Description = "Test Description",
            Priority = PriorityEnums.High,
            CurrentUserId = Guid.NewGuid(),
            AttachmentUrl = "https://example.com/attachment"
        };

        var category = new RequestCategory
        {
            CategoryId = command.CategoryId,
            CategoryName = "Test Category",
            IsActive = false,
            SLAHours = 24,
            IsApprovalRequired = false
        };

        _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(command.CategoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Category '{category.CategoryName}' is not active and cannot accept new requests.");
    }
}