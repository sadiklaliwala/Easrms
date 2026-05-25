using Easrms.Application.Interfaces.OAuth;
using Easrms.Common.Constants;
using Easrms.Domain.Entities;
using Easrms.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Easrms.Infrastructure.Repositories.Implementations;

public class AuthProviderRepository : IAuthProviderRepository
{
    private readonly AppDbContext _db;

    public AuthProviderRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(UserAuthProvider entity)
    {
        await _db.UserAuthProviders.AddAsync(entity);
        await _db.SaveChangesAsync();
    }

    public async Task<int> CountByUserIdAsync(Guid userId)
    {
        return await _db.UserAuthProviders.CountAsync(x => x.UserId == userId);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _db.UserAuthProviders.FindAsync(id);
        if (entity != null)
        {
            _db.UserAuthProviders.Remove(entity);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<UserAuthProvider?> GetByUserIdAndProviderAsync(Guid userId, AuthProviderEnum provider)
    {
        return await _db.UserAuthProviders.FirstOrDefaultAsync(x => x.UserId == userId && x.AuthProvider == provider);
    }

    public async Task<List<UserAuthProvider>> GetByUserIdAsync(Guid userId)
    {
        return await _db.UserAuthProviders.Where(x => x.UserId == userId).ToListAsync();
    }
}
