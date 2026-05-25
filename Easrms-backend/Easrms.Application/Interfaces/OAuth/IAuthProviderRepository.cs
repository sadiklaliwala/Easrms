using Easrms.Domain.Entities;
using Easrms.Common.Constants;

namespace Easrms.Application.Interfaces.OAuth;

public interface IAuthProviderRepository
{
    Task<List<UserAuthProvider>> GetByUserIdAsync(Guid userId);
    Task<UserAuthProvider?> GetByUserIdAndProviderAsync(Guid userId, AuthProviderEnum provider);
    Task AddAsync(UserAuthProvider entity);
    Task DeleteAsync(Guid id);
    Task<int> CountByUserIdAsync(Guid userId);
}
