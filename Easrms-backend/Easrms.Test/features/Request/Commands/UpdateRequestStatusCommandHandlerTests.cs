using System;
using System.Threading;
using System.Threading.Tasks;
using Easrms.Application.Features.Request.Commands;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Common.Enums;
using Easrms.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using Easrms.Application.Interfaces.Email;

namespace Easrms.Test.features.Request.Commands;

public class UpdateRequestStatusCommandHandlerTests
{
    private readonly Mock<IRequestRepository> _reqRepo = new();
    private readonly Mock<ICommentRepository> _commentRepo = new();
    private readonly Mock<IUserRepository> _user_repo = new();
    private readonly Mock<IEmailService> _emailService = new();
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<UpdateRequestStatusCommandHandler>> _logger = new();
    private readonly UpdateRequestStatusCommandHandler _handler;

    public UpdateRequestStatusCommandHandlerTests()
    {
        _handler = new UpdateRequestStatusCommandHandler(_reqRepo.Object, _commentRepo.Object, _user_repo.Object, _emailService.Object, _logger.Object);
    }

    [Fact]
    public async Task Should_MoveToInProgress_When_AssignedByAssignedUser()
    {
        var reqId = Guid.NewGuid();
        var assignedUserId = Guid.NewGuid();
        var entity = new ServiceRequest { RequestId = reqId, Status = RequestStatusEnum.Assigned, AssignedTo = assignedUserId, Employee = new User { Email = "emp@e.com" }, Category = new RequestCategory { SLAHours = 4 }, DueDate = DateTime.UtcNow.AddHours(1) };
        _reqRepo.Setup(r => r.GetRequestByIdAsync(reqId, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        _reqRepo.Setup(r => r.Update(It.IsAny<ServiceRequest>())).Verifiable();
        _commentRepo.Setup(c => c.AddStatusHistoryAsync(It.IsAny<RequestStatusHistory>(), It.IsAny<CancellationToken>())).Verifiable();
        _reqRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new UpdateRequestStatusCommand { RequestId = reqId, NewStatus = RequestStatusEnum.InProgress, CurrentUserId = assignedUserId };

        await _handler.Handle(command, CancellationToken.None);

        _reqRepo.Verify(r => r.Update(It.Is<ServiceRequest>(s => s.Status == RequestStatusEnum.InProgress)), Times.Once);
    }

    [Fact]
    public async Task Should_ThrowNotFound_When_Missing()
    {
        var reqId = Guid.NewGuid();
        _reqRepo.Setup(r => r.GetRequestByIdAsync(reqId, It.IsAny<CancellationToken>())).ReturnsAsync((ServiceRequest?)null);
        var command = new UpdateRequestStatusCommand { RequestId = reqId, NewStatus = RequestStatusEnum.InProgress, CurrentUserId = Guid.NewGuid() };
        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None)).Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Should_ThrowInvalidOperation_When_InvalidTransition()
    {
        var reqId = Guid.NewGuid();
        var entity = new ServiceRequest { RequestId = reqId, Status = RequestStatusEnum.Open, AssignedTo = Guid.NewGuid() };
        _reqRepo.Setup(r => r.GetRequestByIdAsync(reqId, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        var command = new UpdateRequestStatusCommand { RequestId = reqId, NewStatus = RequestStatusEnum.Resolved, CurrentUserId = entity.AssignedTo.Value };
        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None)).Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Should_ThrowUnauthorized_When_NotAssignedUser()
    {
        var reqId = Guid.NewGuid();
        var entity = new ServiceRequest { RequestId = reqId, Status = RequestStatusEnum.Assigned, AssignedTo = Guid.NewGuid() };
        _reqRepo.Setup(r => r.GetRequestByIdAsync(reqId, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        var command = new UpdateRequestStatusCommand { RequestId = reqId, NewStatus = RequestStatusEnum.InProgress, CurrentUserId = Guid.NewGuid() };
        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None)).Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Should_ThrowInvalidOperation_When_ResolveWithoutRemarks()
    {
        var reqId = Guid.NewGuid();
        var assignedUserId = Guid.NewGuid();
        var entity = new ServiceRequest { RequestId = reqId, Status = RequestStatusEnum.InProgress, AssignedTo = assignedUserId };
        _reqRepo.Setup(r => r.GetRequestByIdAsync(reqId, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        var command = new UpdateRequestStatusCommand { RequestId = reqId, NewStatus = RequestStatusEnum.Resolved, Remarks = "", CurrentUserId = assignedUserId };
        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None)).Should().ThrowAsync<InvalidOperationException>();
    }
}
