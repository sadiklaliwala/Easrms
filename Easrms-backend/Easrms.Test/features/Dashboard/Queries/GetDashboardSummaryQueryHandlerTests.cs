using System;
using System.Threading;
using System.Threading.Tasks;
using Easrms.Application.Features.Dashboard.Queries;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Application.DTOs.Dashboard;
using FluentAssertions;
using Moq;
using Xunit;

namespace Easrms.Test.features.Dashboard.Queries;

public class GetDashboardSummaryQueryHandlerTests
{
    private readonly Mock<IDashboardRepository> _dashRepo = new();
    private readonly GetDashboardSummaryQueryHandler _handler;

    public GetDashboardSummaryQueryHandlerTests()
    {
        _handler = new GetDashboardSummaryQueryHandler(_dashRepo.Object);
    }

    [Fact]
    public async Task Should_CallRepository_WithEmployeeScope_When_EmployeeRole()
    {
        var dto = new DashboardSummaryDto();
        _dashRepo.Setup(d => d.GetSummaryAsync(It.IsAny<DashboardQueryParams>(), It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        var query = new GetDashboardSummaryQuery { CurrentUserId = Guid.NewGuid(), RoleName = "Employee" };
        var res = await _handler.Handle(query, CancellationToken.None);
        res.Should().Be(dto);
        _dashRepo.Verify(d => d.GetSummaryAsync(It.Is<DashboardQueryParams>(p => p.EmployeeId == query.CurrentUserId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_CallRepository_WithAdminScope_When_AdminRole()
    {
        var dto = new DashboardSummaryDto();
        _dashRepo.Setup(d => d.GetSummaryAsync(It.IsAny<DashboardQueryParams>(), It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        var query = new GetDashboardSummaryQuery { CurrentUserId = Guid.NewGuid(), RoleName = "Admin" };
        var res = await _handler.Handle(query, CancellationToken.None);
        res.Should().Be(dto);
        _dashRepo.Verify(d => d.GetSummaryAsync(It.Is<DashboardQueryParams>(p => p.EmployeeId == null && p.ManagerId == null && p.AssignedToUserId == null), It.IsAny<CancellationToken>()), Times.Once);
    }
}
