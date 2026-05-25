using System;
using System.Threading;
using System.Threading.Tasks;
using Easrms.Application.Features.Request.Queries;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Application.DTOs.Request;
using FluentAssertions;
using Moq;
using Xunit;

namespace Easrms.Test.features.Request.Queries;

public class GetAllRequestsQueryHandlerTests
{
    private readonly Mock<IRequestRepository> _reqRepo = new();
    private readonly GetAllRequestsQueryHandler _handler;

    public GetAllRequestsQueryHandlerTests()
    {
        _handler = new GetAllRequestsQueryHandler(_reqRepo.Object);
    }

    [Fact]
    public async Task Should_ReturnPaged_When_Admin()
    {
        var dto = new RequestListWithPaginationDto { Items = new System.Collections.Generic.List<RequestListDto>() };
        _reqRepo.Setup(r => r.GetPagedRequestsAsync(It.IsAny<RequestQueryParams>(), It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        var query = new GetAllRequestsQuery { CurrentUserRole = "Admin", CurrentUserId = Guid.NewGuid() };
        var res = await _handler.Handle(query, CancellationToken.None);
        res.Should().BeSameAs(dto);
    }

    [Fact]
    public async Task Should_ScopeAsEmployee_When_EmployeeRole()
    {
        var dto = new RequestListWithPaginationDto { Items = new System.Collections.Generic.List<RequestListDto>() };
        _reqRepo.Setup(r => r.GetPagedRequestsAsync(It.Is<RequestQueryParams>(q => q.EmployeeId != null), It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        var query = new GetAllRequestsQuery { CurrentUserRole = "Employee", CurrentUserId = Guid.NewGuid() };
        var res = await _handler.Handle(query, CancellationToken.None);
        res.Should().BeSameAs(dto);
    }
}
