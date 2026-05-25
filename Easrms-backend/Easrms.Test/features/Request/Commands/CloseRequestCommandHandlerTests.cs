using System;
using System.Threading;
using System.Threading.Tasks;
using Easrms.Application.Features.Request.Commands;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Common.Constants;
using Easrms.Common.Enums;
using Easrms.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace Easrms.Test.features.Request.Commands;

public class CloseRequestCommandHandlerTests
{
    private readonly Mock<IRequestRepository> _reqRepo = new();
    private readonly Mock<ICommentRepository> _commentRepo = new();
    private readonly CloseRequestCommandHandler _handler;

    public CloseRequestCommandHandlerTests()
    {
        _handler = new CloseRequestCommandHandler(_reqRepo.Object, _commentRepo.Object);
    }

    [Fact]
    public async Task Should_Close_When_ResolvedAndOwner()
    {
        var reqId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var entity = new ServiceRequest { RequestId = reqId, Status = RequestStatusEnum.Resolved, EmployeeId = ownerId };
        _reqRepo.Setup(r => r.GetRequestByIdAsync(reqId, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        _reqRepo.Setup(r => r.Update(It.IsAny<ServiceRequest>())).Verifiable();
        _commentRepo.Setup(c => c.AddStatusHistoryAsync(It.IsAny<RequestStatusHistory>(), It.IsAny<CancellationToken>())).Verifiable();
        _reqRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new CloseRequestCommand { RequestId = reqId, CurrentUserId = ownerId, CurrentUserRole = RoleConstants.Employee };
        await _handler.Handle(command, CancellationToken.None);

        _reqRepo.Verify(r => r.Update(It.Is<ServiceRequest>(s => s.Status == RequestStatusEnum.Closed && s.ClosedBy == ownerId)), Times.Once);
    }

    [Fact]
    public async Task Should_ThrowNotFound_When_Missing()
    {
        var reqId = Guid.NewGuid();
        _reqRepo.Setup(r => r.GetRequestByIdAsync(reqId, It.IsAny<CancellationToken>())).ReturnsAsync((ServiceRequest?)null);
        var command = new CloseRequestCommand { RequestId = reqId, CurrentUserId = Guid.NewGuid(), CurrentUserRole = RoleConstants.Employee };
        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None)).Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Should_ThrowInvalidOperation_When_NotResolved()
    {
        var reqId = Guid.NewGuid();
        var entity = new ServiceRequest { RequestId = reqId, Status = RequestStatusEnum.Open, EmployeeId = Guid.NewGuid() };
        _reqRepo.Setup(r => r.GetRequestByIdAsync(reqId, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        var command = new CloseRequestCommand { RequestId = reqId, CurrentUserId = Guid.NewGuid(), CurrentUserRole = RoleConstants.Employee };
        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None)).Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Should_ThrowUnauthorized_When_NotAdminOrOwner()
    {
        var reqId = Guid.NewGuid();
        var entity = new ServiceRequest { RequestId = reqId, Status = RequestStatusEnum.Resolved, EmployeeId = Guid.NewGuid() };
        _reqRepo.Setup(r => r.GetRequestByIdAsync(reqId, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        var command = new CloseRequestCommand { RequestId = reqId, CurrentUserId = Guid.NewGuid(), CurrentUserRole = RoleConstants.SupportUser };
        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None)).Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
