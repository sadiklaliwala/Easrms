using Dapper;
using Easrms.Application.DTOs.Comment;
using Easrms.Application.Interfaces.Repositories;
using Easrms.Common.Enums;
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
    /// CommentType is stored as string in DB — returned as-is in DTO.
    /// </summary>
    public async Task<IReadOnlyList<CommentListDto>> GetCommentsByRequestIdAsync(
        Guid requestId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT
                rc.CommentId,
                rc.CommentText,
                rc.CommentType,
                u.FullName AS CommentByName,
                rc.CreatedOn
            FROM RequestComments rc
            LEFT JOIN Users u ON rc.CommentBy = u.UserId
            WHERE rc.RequestId = @RequestId AND rc.IsDeleted = 0
            ORDER BY rc.CreatedOn ASC;";

        using var conn = _dapperContext.CreateConnection();
        var rows = (await conn.QueryAsync(
            new CommandDefinition(sql, new { RequestId = requestId }, cancellationToken: cancellationToken)
        )).ToList();

        return rows.Select(row => new CommentListDto
        {
            CommentId = (Guid)row.CommentId,
            CommentText = row.CommentText ?? string.Empty,
            CommentType = row.CommentType ?? string.Empty,  // string as-is
            CommentByName = row.CommentByName ?? string.Empty,
            CreatedOn = (DateTime)row.CreatedOn
        }).ToList();
    }

    /// <summary>
    /// Lightweight existence + ownership check for a comment.
    /// </summary>
    public async Task<bool> CommentExistsAsync(
        Guid commentId,
        Guid requestId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM RequestComments
                WHERE CommentId = @CommentId AND RequestId = @RequestId
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END";

        using var conn = _dapperContext.CreateConnection();
        return await conn.ExecuteScalarAsync<bool>(
            new CommandDefinition(sql, new { CommentId = commentId, RequestId = requestId }, cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Inserts a new RequestComment. Handler must call SaveChangesAsync.
    /// </summary>
    public async Task AddCommentAsync(RequestComment comment, CancellationToken cancellationToken = default)
    {
        if (comment is null) throw new ArgumentNullException(nameof(comment));
        if (comment.CreatedOn == default) comment.CreatedOn = DateTime.UtcNow;
        await _dbContext.RequestComments.AddAsync(comment, cancellationToken);
    }

    /// <summary>
    /// Returns status history for a request ordered by ChangedOn ASC.
    /// OldStatus and NewStatus are stored as int in DB — cast directly to enum.
    /// </summary>
    public async Task<IReadOnlyList<StatusHistoryDto>> GetStatusHistoryByRequestIdAsync(
        Guid requestId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT
                h.HistoryId,
                h.OldStatus,
                h.NewStatus,
                u.FullName AS ChangedByName,
                h.ChangedOn,
                h.Remarks
            FROM RequestStatusHistories h
            LEFT JOIN Users u ON h.ChangedBy = u.UserId
            WHERE h.RequestId = @RequestId
            ORDER BY h.ChangedOn ASC;";

        using var conn = _dapperContext.CreateConnection();
        var rows = (await conn.QueryAsync(
            new CommandDefinition(sql, new { RequestId = requestId }, cancellationToken: cancellationToken)
        )).ToList();

        return rows.Select(row =>
        {
            // OldStatus is nullable int in DB
            RequestStatusEnum? oldStatus = null;
            if (row.OldStatus is not null)
            {
                int oldInt = (int)row.OldStatus;
                if (Enum.IsDefined(typeof(RequestStatusEnum), oldInt))
                    oldStatus = (RequestStatusEnum)oldInt;
            }

            // NewStatus is non-nullable int in DB
            int newInt = (int)row.NewStatus;
            var newStatus = Enum.IsDefined(typeof(RequestStatusEnum), newInt)
                ? (RequestStatusEnum)newInt
                : RequestStatusEnum.Open;

            return new StatusHistoryDto
            {
                HistoryId = (Guid)row.HistoryId,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                ChangedByName = row.ChangedByName ?? string.Empty,
                ChangedOn = (DateTime)row.ChangedOn,
                Remarks = row.Remarks ?? string.Empty
            };
        }).ToList();
    }

    /// <summary>
    /// Adds a status history entry. Handler must call SaveChangesAsync.
    /// </summary>
    public async Task AddStatusHistoryAsync(RequestStatusHistory history, CancellationToken cancellationToken = default)
    {
        if (history is null) throw new ArgumentNullException(nameof(history));
        if (history.ChangedOn == default) history.ChangedOn = DateTime.UtcNow;
        await _dbContext.RequestStatusHistories.AddAsync(history, cancellationToken);
    }

    /// <summary>
    /// Persists pending EF Core changes. Called by handlers to commit unit of work.
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}