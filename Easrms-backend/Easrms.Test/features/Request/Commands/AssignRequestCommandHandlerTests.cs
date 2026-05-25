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

public class AssignRequestCommandHandlerTests
{
    private readonly Mock<IRequestRepository> _reqRepo = new();
    private readonly Mock<ICommentRepository> _commentRepo = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly AssignRequestCommandHandler _handler;

    public AssignRequestCommandHandlerTests()
    {
        _handler = new AssignRequestCommandHandler(_reqRepo.Object, _commentRepo.Object, _userRepo.Object);
    }

    [Fact]
    public async Task Should_Assign_When_StatusOpenAndUserExists()
    {
        var reqId = Guid.NewGuid();
        var supportUserId = Guid.NewGuid();
        var entity = new ServiceRequest { RequestId = reqId, Status = RequestStatusEnum.Open, RequestNumber = "REQ-1" };

        _reqRepo.Setup(r => r.GetRequestByIdAsync(reqId, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        _userRepo.Setup(u => u.ExistsAsync(supportUserId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _reqRepo.Setup(r => r.Update(It.IsAny<ServiceRequest>())).Verifiable();
        _commentRepo.Setup(c => c.AddStatusHistoryAsync(It.IsAny<RequestStatusHistory>(), It.IsAny<CancellationToken>())).Verifiable();
        _reqRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new AssignRequestCommand { RequestId = reqId, SupportUserId = supportUserId, CurrentUserId = Guid.NewGuid() };
        await _handler.Handle(command, CancellationToken.None);

        _reqRepo.Verify(r => r.Update(It.Is<ServiceRequest>(s => s.AssignedTo == supportUserId && s.Status == RequestStatusEnum.Assigned)), Times.Once);
    }

    [Fact]
    public async Task Should_ThrowNotFound_When_RequestMissing()
    {
        var reqId = Guid.NewGuid();
        _reqRepo.Setup(r => r.GetRequestByIdAsync(reqId, It.IsAny<CancellationToken>())).ReturnsAsync((ServiceRequest?)null);
        var command = new AssignRequestCommand { RequestId = reqId, SupportUserId = Guid.NewGuid(), CurrentUserId = Guid.NewGuid() };
        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None)).Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Should_ThrowInvalidOperation_When_StatusNotAssignable()
    {
        var reqId = Guid.NewGuid();
        var entity = new ServiceRequest { RequestId = reqId, Status = RequestStatusEnum.Closed, RequestNumber = "REQ-1" };
        _reqRepo.Setup(r => r.GetRequestByIdAsync(reqId, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        var command = new AssignRequestCommand { RequestId = reqId, SupportUserId = Guid.NewGuid(), CurrentUserId = Guid.NewGuid() };
        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None)).Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Should_ThrowNotFound_When_SupportUserMissing()
    {
        var reqId = Guid.NewGuid();
        var supportUserId = Guid.NewGuid();
        var entity = new ServiceRequest { RequestId = reqId, Status = RequestStatusEnum.Open, RequestNumber = "REQ-1" };
        _reqRepo.Setup(r => r.GetRequestByIdAsync(reqId, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        _userRepo.Setup(u => u.ExistsAsync(supportUserId, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var command = new AssignRequestCommand { RequestId = reqId, SupportUserId = supportUserId, CurrentUserId = Guid.NewGuid() };
        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None)).Should().ThrowAsync<KeyNotFoundException>();
    }
}
