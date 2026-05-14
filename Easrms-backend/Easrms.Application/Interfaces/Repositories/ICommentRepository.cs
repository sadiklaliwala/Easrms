// Easrms.Infrastructure/Repositories/Interfaces/ICommentRepository.cs

using Easrms.Application.DTOs.Comment;
using Easrms.Domain.Entities;

namespace Easrms.Application.Interfaces.Repositories;

/// <summary>
/// Repository contract for all <see cref="RequestComment"/> and <see cref="RequestStatusHistory"/> data operations.
/// Both entities are scoped under this single repository because they are always accessed
/// in the context of a parent <see cref="ServiceRequest"/> and never in isolation.
///
/// Read operations use Dapper for join-heavy queries (CommentByName, ChangedByName).
/// Write operations use EF Core.
/// All async methods accept a <see cref="CancellationToken"/> for request cancellation support.
/// </summary>
public interface ICommentRepository
{
    // ─────────────────────────────────────────────────────────────────────────
    // COMMENT — QUERY METHODS
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns all non-deleted comments for a given request, ordered by CreatedOn ASC
    /// so the conversation thread reads chronologically top to bottom.
    /// Uses Dapper to join Users table for CommentByName in a single query.
    /// Soft-deleted comments (IsDeleted = true) are excluded automatically.
    /// No pagination — comment lists are expected to be short in this domain.
    /// </summary>
    /// <param name="requestId">The unique identifier of the parent request.</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>
    /// An ordered list of <see cref="CommentListDto"/>. Returns empty list if none exist.
    /// </returns>
    Task<IReadOnlyList<CommentListDto>> GetCommentsByRequestIdAsync(
        Guid requestId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a comment with the given ID exists and belongs to the given request.
    /// Used as a combined existence + ownership guard before any comment mutation.
    /// Lightweight — executes a single SQL EXISTS check, no entity materialization.
    /// </summary>
    /// <param name="commentId">The unique identifier of the comment.</param>
    /// <param name="requestId">The request the comment must belong to.</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>
    /// <c>true</c> if the comment exists and belongs to the request; otherwise <c>false</c>.
    /// </returns>
    Task<bool> CommentExistsAsync(
        Guid commentId,
        Guid requestId,
        CancellationToken cancellationToken = default);

    // ─────────────────────────────────────────────────────────────────────────
    // COMMENT — COMMAND METHODS
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Inserts a new <see cref="RequestComment"/> record into the database.
    /// The entity must have RequestId, CommentBy, CommentText, CommentType,
    /// CreatedOn, and IsDeleted = false already set by the command handler.
    /// Does not call SaveChanges — SaveChangesAsync is called by the handler.
    /// </summary>
    /// <param name="comment">The fully constructed <see cref="RequestComment"/> entity to insert.</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    Task AddCommentAsync(
        RequestComment comment,
        CancellationToken cancellationToken = default);

    // ─────────────────────────────────────────────────────────────────────────
    // STATUS HISTORY — QUERY METHODS
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the full status history for a given request, ordered by ChangedOn ASC
    /// so the timeline reads from first action to most recent.
    /// Uses Dapper to join Users table for ChangedByName in a single query.
    /// </summary>
    /// <param name="requestId">The unique identifier of the parent request.</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>
    /// An ordered list of <see cref="StatusHistoryDto"/>. Returns empty list if none exist.
    /// </returns>
    Task<IReadOnlyList<StatusHistoryDto>> GetStatusHistoryByRequestIdAsync(
        Guid requestId,
        CancellationToken cancellationToken = default);

    // ─────────────────────────────────────────────────────────────────────────
    // STATUS HISTORY — COMMAND METHODS
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Inserts a new <see cref="RequestStatusHistory"/> audit record into the database.
    /// Called by every command handler that mutates request status:
    /// Create, Approve, Reject, Assign, UpdateStatus (InProgress / Resolved), and Close.
    /// The entity must have RequestId, OldStatus, NewStatus, ChangedBy, ChangedOn,
    /// and optionally Remarks already set by the handler.
    /// Does not call SaveChanges — SaveChangesAsync is called by the handler
    /// once for both the status history insert and the request update in the same transaction.
    /// </summary>
    /// <param name="history">The fully constructed <see cref="RequestStatusHistory"/> entity to insert.</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    Task AddStatusHistoryAsync(
        RequestStatusHistory history,
        CancellationToken cancellationToken = default);

    // ─────────────────────────────────────────────────────────────────────────
    // SHARED — UNIT OF WORK
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Persists all pending EF Core change tracker entries to the database.
    /// Called once at the end of each command handler to commit the unit of work.
    /// Covers both <see cref="RequestComment"/> and <see cref="RequestStatusHistory"/> inserts
    /// in the same transaction when both are added in the same handler.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>The number of rows affected.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}