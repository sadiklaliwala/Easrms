// Easrms.Infrastructure/Repositories/Implementations/DashboardRepository.cs

// Easrms.Infrastructure/Repositories/Implementations/DashboardRepository.cs
using Easrms.Application.DTOs.Dashboard;

namespace Easrms.Application.Interfaces.Repositories
{
    public interface IDashboardRepository
    {
        Task<IReadOnlyList<CategoryCountDto>> GetCategoryCountsAsync(DashboardQueryParams queryParams, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<PriorityCountDto>> GetPriorityCountsAsync(DashboardQueryParams queryParams, CancellationToken cancellationToken = default);
        Task<IReadOnlyDictionary<int, int>> GetStatusCountsAsync(DashboardQueryParams queryParams, CancellationToken cancellationToken = default);
        Task<DashboardSummaryDto> GetSummaryAsync(DashboardQueryParams queryParams, CancellationToken cancellationToken = default);
    }
}