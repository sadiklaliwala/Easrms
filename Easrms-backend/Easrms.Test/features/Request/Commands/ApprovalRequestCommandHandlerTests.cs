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

namespace Easrms.Test.features.Request.Commands;

public class ApprovalRequestCommandHandlerTests
{
    private readonly Mock<IRequestRepository> _reqRepo = new();
    private readonly Mock<ICommentRepository> _commentRepo = new();
    private readonly ApprovalRequestCommandHandler _handler;

    public ApprovalRequestCommandHandlerTests()
    {
        _handler = new ApprovalRequestCommandHandler(_reqRepo.Object, _commentRepo.Object);
    }

    [Fact]
    public async Task Should_Approve_When_RequestIsPendingAndManagerMatches()
    {
        var reqId = Guid.NewGuid();
        var managerId = Guid.NewGuid();

        var entity = new ServiceRequest
        {
            RequestId = reqId,
            RequestNumber = "REQ-1",
            Status = RequestStatusEnum.PendingApproval,
            Employee = new User { ManagerId = managerId },
        };

        _reqRepo.Setup(r => r.GetRequestByIdAsync(reqId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        _reqRepo.Setup(r => r.Update(It.IsAny<ServiceRequest>())).Verifiable();
        _commentRepo.Setup(c => c.AddStatusHistoryAsync(It.IsAny<RequestStatusHistory>(), It.IsAny<CancellationToken>())).Verifiable();
        _commentRepo.Setup(c => c.AddCommentAsync(It.IsAny<RequestComment>(), It.IsAny<CancellationToken>())).Verifiable();
        _reqRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new ApprovalRequestCommand { RequestId = reqId, Action = "Approve", CurrentUserId = managerId };

        await _handler.Handle(command, CancellationToken.None);

        _reqRepo.Verify(r => r.Update(It.Is<ServiceRequest>(s => s.Status == RequestStatusEnum.Approved)), Times.Once);
        _commentRepo.Verify(c => c.AddStatusHistoryAsync(It.IsAny<RequestStatusHistory>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_ThrowNotFound_When_RequestMissing()
    {
        var reqId = Guid.NewGuid();
        _reqRepo.Setup(r => r.GetRequestByIdAsync(reqId, It.IsAny<CancellationToken>())).ReturnsAsync((ServiceRequest?)null);

        var command = new ApprovalRequestCommand { RequestId = reqId, Action = "Approve", CurrentUserId = Guid.NewGuid() };

        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Should_ThrowInvalidOperation_When_NotPendingApproval()
    {
        var reqId = Guid.NewGuid();
        var entity = new ServiceRequest { RequestId = reqId, Status = RequestStatusEnum.Open, Employee = new User { ManagerId = Guid.NewGuid() } };
        _reqRepo.Setup(r => r.GetRequestByIdAsync(reqId, It.IsAny<CancellationToken>())).ReturnsAsync(entity);

        var command = new ApprovalRequestCommand { RequestId = reqId, Action = "Approve", CurrentUserId = Guid.NewGuid() };

        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Should_ThrowUnauthorized_When_ManagerMismatch()
    {
        var reqId = Guid.NewGuid();
        var entity = new ServiceRequest { RequestId = reqId, Status = RequestStatusEnum.PendingApproval, Employee = new User { ManagerId = Guid.NewGuid() } };
        _reqRepo.Setup(r => r.GetRequestByIdAsync(reqId, It.IsAny<CancellationToken>())).ReturnsAsync(entity);

        var command = new ApprovalRequestCommand { RequestId = reqId, Action = "Approve", CurrentUserId = Guid.NewGuid() };

        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Should_ThrowInvalidOperation_When_RejectWithoutComment()
    {
        var reqId = Guid.NewGuid();
        var managerId = Guid.NewGuid();
        var entity = new ServiceRequest { RequestId = reqId, Status = RequestStatusEnum.PendingApproval, Employee = new User { ManagerId = managerId } };
        _reqRepo.Setup(r => r.GetRequestByIdAsync(reqId, It.IsAny<CancellationToken>())).ReturnsAsync(entity);

        var command = new ApprovalRequestCommand { RequestId = reqId, Action = "Reject", Comment = "", CurrentUserId = managerId };

        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>();
    }
}
