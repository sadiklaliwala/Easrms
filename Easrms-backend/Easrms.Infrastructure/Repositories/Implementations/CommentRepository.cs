using Dapper;
using Easrms.Application.DTOs.Comment;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Domain.Entities;
using Easrms.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Easrms.Infrastructure.Repositories.Implementations;

/// <summary>
/// Repository for comments and status history.
/// - Dapper for read queries that join Users for display names.
/// - EF Core for inserts and unit-of-work operations.
/// </summary>
public class CommentRepository : ICommentRepository
{
    private readonly AppDbContext _dbContext;
    private readonly DapperContext _dapperContext;

    public CommentRepository(AppDbContext dbContext, DapperContext dapperContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _dapperContext = dapperContext ?? throw new ArgumentNullException(nameof(dapperContext));
    }

    /// <summary>
    /// Returns non-deleted comments for a request ordered by CreatedOn ASC.
    /// Uses Dapper to join the Users table and return DTOs.
    /// </summary>
    public async Task<IReadOnlyList<CommentListDto>> GetCommentsByRequestIdAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        const string sql = @"SELECT rc.CommentId, rc.CommentText, rc.CommentType, u.FullName AS CommentByName, rc.CreatedOn
FROM RequestComments rc
LEFT JOIN Users u ON rc.CommentBy = u.UserId
WHERE rc.RequestId = @RequestId AND rc.IsDeleted = 0
ORDER BY rc.CreatedOn ASC;";

        using var conn = _dapperContext.CreateConnection();
        var rows = await conn.QueryAsync(new CommandDefinition(sql, new { RequestId = requestId }, cancellationToken: cancellationToken));

        var list = new List<CommentListDto>();
        foreach (var row in rows)
        {
            // CommentType in the database is stored as string; attempt parse to int for DTO
            int commentType = 0;
            try
            {
                var ct = (row.CommentType as object)?.ToString();
                if (!string.IsNullOrEmpty(ct)) int.TryParse(ct, out commentType);
            }
            catch { /* ignore and leave default 0 */ }

            list.Add(new CommentListDto
            {
                CommentId = row.CommentId,
                CommentText = row.CommentText ?? string.Empty,
                CommentType = commentType,
                CommentByName = row.CommentByName ?? string.Empty,
                CreatedOn = row.CreatedOn
            });
        }

        return list;
    }

    /// <summary>
    /// Lightweight existence + ownership check for a comment.
    /// Uses Dapper scalar EXISTS for minimal overhead.
    /// </summary>
    public async Task<bool> CommentExistsAsync(Guid commentId, Guid requestId, CancellationToken cancellationToken = default)
    {
        const string sql = @"SELECT CASE WHEN EXISTS(
    SELECT 1 FROM RequestComments rc WHERE rc.CommentId = @CommentId AND rc.RequestId = @RequestId
) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END";

        using var conn = _dapperContext.CreateConnection();
        var exists = await conn.ExecuteScalarAsync<bool>(new CommandDefinition(sql, new { CommentId = commentId, RequestId = requestId }, cancellationToken: cancellationToken));
        return exists;
    }

    /// <summary>
    /// Inserts a new RequestComment into the EF Core DbContext. Handler must call SaveChangesAsync.
    /// </summary>
    public async Task AddCommentAsync(RequestComment comment, CancellationToken cancellationToken = default)
    {
        if (comment is null) throw new ArgumentNullException(nameof(comment));

        // Ensure CreatedOn is UTC
        if (comment.CreatedOn == default) comment.CreatedOn = DateTime.UtcNow;

        await _dbContext.RequestComments.AddAsync(comment, cancellationToken);
    }

    /// <summary>
    /// Returns status history entries for a request ordered by ChangedOn ASC.
    /// Uses Dapper to join users for ChangedByName.
    /// </summary>
    public async Task<IReadOnlyList<StatusHistoryDto>> GetStatusHistoryByRequestIdAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        const string sql = @"SELECT h.HistoryId, h.OldStatus, h.NewStatus, u.FullName AS ChangedByName, h.ChangedOn, h.Remarks
FROM RequestStatusHistories h
LEFT JOIN Users u ON h.ChangedBy = u.UserId
WHERE h.RequestId = @RequestId
ORDER BY h.ChangedOn ASC;";

        using var conn = _dapperContext.CreateConnection();
        var rows = await conn.QueryAsync(new CommandDefinition(sql, new { RequestId = requestId }, cancellationToken: cancellationToken));

        var list = new List<StatusHistoryDto>();
        foreach (var row in rows)
        {
            int? oldStatus = null;
            int newStatus = 0;
            try
            {
                var os = (row.OldStatus as object)?.ToString();
                if (!string.IsNullOrEmpty(os) && int.TryParse(os, out var o)) oldStatus = o;
            }
            catch { }

            try
            {
                var ns = (row.NewStatus as object)?.ToString();
                if (!string.IsNullOrEmpty(ns)) int.TryParse(ns, out newStatus);
            }
            catch { }

            list.Add(new StatusHistoryDto
            {
                HistoryId = row.HistoryId,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                ChangedByName = row.ChangedByName ?? string.Empty,
                ChangedOn = row.ChangedOn,
                Remarks = row.Remarks
            });
        }

        return list;
    }

    /// <summary>
    /// Adds a status history entry to the DbContext. Handler must call SaveChangesAsync.
    /// </summary>
    public async Task AddStatusHistoryAsync(RequestStatusHistory history, CancellationToken cancellationToken = default)
    {
        if (history is null) throw new ArgumentNullException(nameof(history));

        if (history.ChangedOn == default) history.ChangedOn = DateTime.UtcNow;

        await _dbContext.RequestStatusHistories.AddAsync(history, cancellationToken);
    }

    /// <summary>
    /// Persists pending EF Core changes. Called by command handlers to commit unit of work.
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

