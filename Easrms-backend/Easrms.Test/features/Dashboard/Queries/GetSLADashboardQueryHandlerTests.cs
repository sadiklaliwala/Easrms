// FileName: GetSLADashboardQueryHandlerTests.cs

using Easrms.Application.DTOs.Dashboard;
using Easrms.Application.Features.Dashboard.Queries;
using Easrms.Application.Interfaces.Repositories;
using FluentAssertions;
using Moq;

namespace Easrms.Test.features.Dashboard.Queries;

public sealed class GetSLADashboardQueryHandlerTests
{
    private readonly Mock<IDashboardRepository> _dashboardRepositoryMock;
    private readonly GetSLADashboardQueryHandler _handler;

    public GetSLADashboardQueryHandlerTests()
    {
        _dashboardRepositoryMock = new Mock<IDashboardRepository>();
        _handler = new GetSLADashboardQueryHandler(_dashboardRepositoryMock.Object);
    }

    private void SetupGetSLASummaryReturns(DashboardSummaryDto result)
    {
        _dashboardRepositoryMock
            .Setup(r => r.GetSLASummaryAsync(It.IsAny<DashboardQueryParams>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
    }

    private static DashboardSummaryDto BuildSLASummaryDto() => new()
    {
        OpenCount = 5,
        WithinSLACount = 3,
        NearingBreachCount = 1,
        BreachedCount = 1,
        EscalatedCount = 0
    };

    // ─── Success — Employee scope ─────────────────────────────────────────────

    [Fact]
    public async Task Should_SetEmployeeId_When_RoleIsEmployee()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetSLADashboardQuery { CurrentUserId = userId, CurrentUserRole = "Employee" };

        DashboardQueryParams? captured = null;
        _dashboardRepositoryMock
            .Setup(r => r.GetSLASummaryAsync(It.IsAny<DashboardQueryParams>(), It.IsAny<CancellationToken>()))
            .Callback<DashboardQueryParams, CancellationToken>((p, _) => captured = p)
            .ReturnsAsync(BuildSLASummaryDto());

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        captured!.EmployeeId.Should().Be(userId);
        captured.ManagerId.Should().BeNull();
        captured.AssignedToUserId.Should().BeNull();
    }

    // ─── Success — Manager scope ──────────────────────────────────────────────

    [Fact]
    public async Task Should_SetManagerId_When_RoleIsManager()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetSLADashboardQuery { CurrentUserId = userId, CurrentUserRole = "Manager" };

        DashboardQueryParams? captured = null;
        _dashboardRepositoryMock
            .Setup(r => r.GetSLASummaryAsync(It.IsAny<DashboardQueryParams>(), It.IsAny<CancellationToken>()))
            .Callback<DashboardQueryParams, CancellationToken>((p, _) => captured = p)
            .ReturnsAsync(BuildSLASummaryDto());

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        captured!.ManagerId.Should().Be(userId);
        captured.EmployeeId.Should().BeNull();
        captured.AssignedToUserId.Should().BeNull();
    }

    // ─── Success — SupportUser scope ─────────────────────────────────────────

    [Fact]
    public async Task Should_SetAssignedToUserId_When_RoleIsSupportUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetSLADashboardQuery { CurrentUserId = userId, CurrentUserRole = "SupportUser" };

        DashboardQueryParams? captured = null;
        _dashboardRepositoryMock
            .Setup(r => r.GetSLASummaryAsync(It.IsAny<DashboardQueryParams>(), It.IsAny<CancellationToken>()))
            .Callback<DashboardQueryParams, CancellationToken>((p, _) => captured = p)
            .ReturnsAsync(BuildSLASummaryDto());

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        captured!.AssignedToUserId.Should().Be(userId);
        captured.EmployeeId.Should().BeNull();
        captured.ManagerId.Should().BeNull();
    }

    // ─── Success — Admin scope (global) ──────────────────────────────────────

    [Fact]
    public async Task Should_SetNoScopeFilter_When_RoleIsAdmin()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetSLADashboardQuery { CurrentUserId = userId, CurrentUserRole = "Admin" };

        DashboardQueryParams? captured = null;
        _dashboardRepositoryMock
            .Setup(r => r.GetSLASummaryAsync(It.IsAny<DashboardQueryParams>(), It.IsAny<CancellationToken>()))
            .Callback<DashboardQueryParams, CancellationToken>((p, _) => captured = p)
            .ReturnsAsync(BuildSLASummaryDto());

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        captured!.EmployeeId.Should().BeNull();
        captured.ManagerId.Should().BeNull();
        captured.AssignedToUserId.Should().BeNull();
    }

    [Fact]
    public async Task Should_SetNoScopeFilter_When_RoleIsUnknown()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetSLADashboardQuery { CurrentUserId = userId, CurrentUserRole = "UnknownRole" };

        DashboardQueryParams? captured = null;
        _dashboardRepositoryMock
            .Setup(r => r.GetSLASummaryAsync(It.IsAny<DashboardQueryParams>(), It.IsAny<CancellationToken>()))
            .Callback<DashboardQueryParams, CancellationToken>((p, _) => captured = p)
            .ReturnsAsync(BuildSLASummaryDto());

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        captured!.EmployeeId.Should().BeNull();
        captured.ManagerId.Should().BeNull();
        captured.AssignedToUserId.Should().BeNull();
    }

    // ─── Return value mapping ─────────────────────────────────────────────────

    [Fact]
    public async Task Should_ReturnSLADashboardDto_WithMappedFields_When_QueryIsHandled()
    {
        // Arrange
        var summary = BuildSLASummaryDto();
        var query = new GetSLADashboardQuery { CurrentUserId = Guid.NewGuid(), CurrentUserRole = "Admin" };

        SetupGetSLASummaryReturns(summary);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalOpen.Should().Be(summary.OpenCount);
        result.WithinSLACount.Should().Be(summary.WithinSLACount);
        result.NearingBreachCount.Should().Be(summary.NearingBreachCount);
        result.BreachedCount.Should().Be(summary.BreachedCount);
        result.EscalatedCount.Should().Be(summary.EscalatedCount);
    }

    // ─── Repository call verification ────────────────────────────────────────

    [Fact]
    public async Task Should_CallGetSLASummaryAsync_Once_When_QueryIsHandled()
    {
        // Arrange
        var query = new GetSLADashboardQuery { CurrentUserId = Guid.NewGuid(), CurrentUserRole = "Admin" };

        SetupGetSLASummaryReturns(BuildSLASummaryDto());

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _dashboardRepositoryMock.Verify(
            r => r.GetSLASummaryAsync(It.IsAny<DashboardQueryParams>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_NeverCallGetSummaryAsync_When_SLAQueryIsHandled()
    {
        // Arrange
        var query = new GetSLADashboardQuery { CurrentUserId = Guid.NewGuid(), CurrentUserRole = "Admin" };

        SetupGetSLASummaryReturns(BuildSLASummaryDto());

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _dashboardRepositoryMock.Verify(
            r => r.GetSummaryAsync(It.IsAny<DashboardQueryParams>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}