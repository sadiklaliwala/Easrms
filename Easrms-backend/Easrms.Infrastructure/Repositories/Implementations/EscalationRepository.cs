using Easrms.Application.Interfaces.Repositories;
using Easrms.Domain.Entities;
using Easrms.Infrastructure.Data;

namespace Easrms.Infrastructure.Repositories.Implementations;

public class EscalationRepository : IEscalationRepository
{
    private readonly AppDbContext _dbContext;

    public EscalationRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task AddEscalationHistoryAsync(RequestEscalationHistory entity, CancellationToken cancellationToken = default)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));
        if (entity.CreatedOn == default) entity.CreatedOn = DateTime.UtcNow;
        if (entity.EscalatedOn == default) entity.EscalatedOn = DateTime.UtcNow;
        await _dbContext.Set<RequestEscalationHistory>().AddAsync(entity, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
