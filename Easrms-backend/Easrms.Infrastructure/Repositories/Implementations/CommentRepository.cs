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
    /// CommentType is stored as int in DB — map to enum or string in DTO as needed.
    /// </summary>
    //public async Task<IReadOnlyList<CommentListDto>> GetCommentsByRequestIdAsync(
    //    Guid requestId,
    //    CancellationToken cancellationToken = default)
    //{
    //    const string sql = @"
    //        SELECT
    //            rc.comment_id AS CommentId,
    //            rc.comment_text AS CommentText,
    //            rc.comment_type AS CommentType,
    //            u.full_name AS CommentByName,
    //            rc.created_on AS CreatedOn
    //        FROM request_comments rc
    //        LEFT JOIN users u ON rc.comment_by = u.user_id
    //        WHERE rc.request_id = @RequestId AND rc.is_deleted = FALSE
    //        ORDER BY rc.created_on ASC;";

    //    using var conn = _dapperContext.CreateConnection();
    //    var rows = (await conn.QueryAsync(
    //        new CommandDefinition(sql, new { RequestId = requestId }, cancellationToken: cancellationToken)
    //    )).ToList();

    //    return rows.Select(row => new CommentListDto
    //    {
    //        CommentId = (Guid)row.CommentId,
    //        CommentText = row.CommentText ?? string.Empty,
    //        // map integer DB value to enum name for DTO
    //        CommentType = Enum.IsDefined(typeof(CommentTypeEnum), (int)row.CommentType)
    //            ? ((CommentTypeEnum)(int)row.CommentType).ToString()
    //            : string.Empty,
    //        CommentByName = row.CommentByName ?? string.Empty,
    //        CreatedOn = (DateTime)row.CreatedOn
    //    }).ToList();
    //}
    public async Task<IReadOnlyList<CommentListDto>> GetCommentsByRequestIdAsync(
    Guid requestId,
    CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT
    rc.comment_id AS ""CommentId"",
    rc.comment_text AS ""CommentText"",
    rc.comment_type AS ""CommentType"",
    u.full_name AS ""CommentByName"",
    rc.created_on AS ""CreatedOn""
FROM request_comments rc
LEFT JOIN users u ON rc.comment_by = u.user_id
WHERE rc.request_id = @RequestId
AND rc.is_deleted = FALSE
ORDER BY rc.created_on ASC;";

        using var conn = _dapperContext.CreateConnection();

        var rows = (await conn.QueryAsync(
            new CommandDefinition(
                sql,
                new { RequestId = requestId },
                cancellationToken: cancellationToken
            )
        )).ToList();

        return rows.Select(row =>
        {
            int commentTypeInt = Convert.ToInt32(row.CommentType ?? 0);

            return new CommentListDto
            {
                CommentId = row.CommentId != null
                    ? (Guid)row.CommentId
                    : Guid.Empty,

                CommentText = row.CommentText?.ToString() ?? string.Empty,

                CommentType = Enum.IsDefined(typeof(CommentTypeEnum), commentTypeInt)
        ? (CommentTypeEnum)commentTypeInt
        : default,

                CommentByName = row.CommentByName?.ToString() ?? string.Empty,

                CreatedOn = row.CreatedOn != null
                    ? (DateTime)row.CreatedOn
                    : DateTime.UtcNow
            };
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
                SELECT 1 FROM request_comments
                WHERE comment_id = @CommentId AND request_id = @RequestId
            ) THEN TRUE ELSE FALSE END";

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
    //public async Task<IReadOnlyList<StatusHistoryDto>> GetStatusHistoryByRequestIdAsync(
    //    Guid requestId,
    //    CancellationToken cancellationToken = default)
    //{
    //    const string sql = @"
    //        SELECT
    //            h.history_id AS HistoryId,
    //            h.old_status AS OldStatus,
    //            h.new_status AS NewStatus,
    //            u.full_name AS ChangedByName,
    //            h.changed_on AS ChangedOn,
    //            h.remarks AS Remarks
    //        FROM request_status_histories h
    //        LEFT JOIN users u ON h.changed_by = u.user_id
    //        WHERE h.request_id = @RequestId
    //        ORDER BY h.changed_on ASC;";

    //    using var conn = _dapperContext.CreateConnection();
    //    var rows = (await conn.QueryAsync(
    //        new CommandDefinition(sql, new { RequestId = requestId }, cancellationToken: cancellationToken)
    //    )).ToList();

    //    return rows.Select(row =>
    //    {
    //        // OldStatus is nullable int in DB
    //        RequestStatusEnum? oldStatus = null;
    //        if (row.OldStatus is not null)
    //        {
    //            int oldInt = (int)row.OldStatus;
    //            if (Enum.IsDefined(typeof(RequestStatusEnum), oldInt))
    //                oldStatus = (RequestStatusEnum)oldInt;
    //        }

    //        // NewStatus is non-nullable int in DB
    //        int newInt = (int)row.NewStatus;
    //        var newStatus = Enum.IsDefined(typeof(RequestStatusEnum), newInt)
    //            ? (RequestStatusEnum)newInt
    //            : RequestStatusEnum.Open;

    //        return new StatusHistoryDto
    //        {
    //            HistoryId = (Guid)row.HistoryId,
    //            OldStatus = oldStatus,
    //            NewStatus = newStatus,
    //            ChangedByName = row.ChangedByName ?? string.Empty,
    //            ChangedOn = (DateTime)row.ChangedOn,
    //            Remarks = row.Remarks ?? string.Empty
    //        };
    //    }).ToList();
    //}
    public async Task<IReadOnlyList<StatusHistoryDto>> GetStatusHistoryByRequestIdAsync(
     Guid requestId,
     CancellationToken cancellationToken = default)
    {
        const string sql = @"
    SELECT
        h.history_id AS ""HistoryId"",
        h.old_status AS ""OldStatus"",
        h.new_status AS ""NewStatus"",
        u.full_name AS ""ChangedByName"",
        h.changed_on AS ""ChangedOn"",
        h.remarks AS ""Remarks""
    FROM request_status_histories h
    LEFT JOIN users u ON h.changed_by = u.user_id
    WHERE h.request_id = @RequestId
    ORDER BY h.changed_on ASC;";

        using var conn = _dapperContext.CreateConnection();

        var rows = (await conn.QueryAsync(
            new CommandDefinition(
                sql,
                new { RequestId = requestId },
                cancellationToken: cancellationToken
            )
        )).ToList();

        return rows.Select(row =>
        {
            RequestStatusEnum? oldStatus = null;

            if (row.OldStatus != null)
            {
                int oldInt = Convert.ToInt32(row.OldStatus);

                if (Enum.IsDefined(typeof(RequestStatusEnum), oldInt))
                {
                    oldStatus = (RequestStatusEnum)oldInt;
                }
            }

            int newInt = Convert.ToInt32(row.NewStatus ?? 0);

            var newStatus = Enum.IsDefined(typeof(RequestStatusEnum), newInt)
                ? (RequestStatusEnum)newInt
                : RequestStatusEnum.Open;

            return new StatusHistoryDto
            {
                HistoryId = row.HistoryId != null
                    ? (Guid)row.HistoryId
                    : Guid.Empty,

                OldStatus = oldStatus,

                NewStatus = newStatus,

                ChangedByName = row.ChangedByName?.ToString() ?? string.Empty,

                ChangedOn = row.ChangedOn != null
                    ? (DateTime)row.ChangedOn
                    : DateTime.UtcNow,

                Remarks = row.Remarks?.ToString() ?? string.Empty
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