using Easrms.Domain.Entities;

namespace Easrms.Application.Interfaces.Repositories;

public interface IEscalationRepository
{
    Task AddEscalationHistoryAsync(RequestEscalationHistory entity, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
