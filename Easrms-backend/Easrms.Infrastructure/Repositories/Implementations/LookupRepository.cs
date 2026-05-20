// Easrms.Infrastructure/Repositories/Implementations/LookupRepository.cs
using Dapper;
using Easrms.Application.DTOs.Lookup;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Common.Constants;
using Easrms.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace Easrms.Infrastructure.Repositories.Implementations;

/// <summary>
/// Lookup repository returns flat lists used by dropdowns.
/// All methods use Dapper and return only minimal DTOs.
/// </summary>
public class LookupRepository : ILookupRepository
{
    private readonly DapperContext _dapperContext;
    private readonly ILogger<LookupRepository> _logger;

    public LookupRepository(DapperContext dapperContext, ILogger<LookupRepository> logger)
    {
        _dapperContext = dapperContext ?? throw new ArgumentNullException(nameof(dapperContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyList<SupportUserLookupDto>> GetActiveSupportUsersAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"SELECT u.UserId, u.FullName
FROM Users u
INNER JOIN Roles r ON u.RoleId = r.RoleId
WHERE u.IsActive = 1 AND r.RoleName = @RoleName
ORDER BY u.FullName ASC;";

        using var conn = _dapperContext.CreateConnection();
        var rows = await conn.QueryAsync<SupportUserLookupDto>(new CommandDefinition(sql, new { RoleName = RoleConstants.SupportUser}, cancellationToken: cancellationToken));
        return rows.AsList();
    }

    public async Task<IReadOnlyList<ManagerLookupDto>> GetActiveManagersAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"SELECT u.UserId, u.FullName
FROM Users u
INNER JOIN Roles r ON u.RoleId = r.RoleId
WHERE u.IsActive = 1 AND r.RoleName = @RoleName
ORDER BY u.FullName ASC;";

        using var conn = _dapperContext.CreateConnection();
        var rows = await conn.QueryAsync<ManagerLookupDto>(new CommandDefinition(sql, new { RoleName = RoleConstants.Manager }, cancellationToken: cancellationToken));
        return rows.AsList();
    }

    public async Task<IReadOnlyList<CategoryLookupDto>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"SELECT rc.CategoryId, rc.CategoryName
FROM RequestCategories rc
WHERE rc.IsActive = 1
ORDER BY rc.CategoryName ASC;";

        using var conn = _dapperContext.CreateConnection();
        var rows = await conn.QueryAsync<CategoryLookupDto>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.AsList();
    }
}

