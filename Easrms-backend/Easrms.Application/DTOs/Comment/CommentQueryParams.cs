// Easrms.Application/DTOs/Comment/CommentQueryParams.cs

namespace Easrms.Application.DTOs.Comment;

/// <summary>
/// Query parameter object for comment and status history read operations.
///
/// Unlike Request and Category, comments and history are always scoped to a single
/// parent RequestId — they are never listed across multiple requests at once.
/// Pagination is intentionally excluded: comment threads in this domain are short
/// and loading them in full is both correct and performant.
///
/// CommentType filter is included to allow the frontend to optionally show only
/// a specific type (e.g. only Resolution comments on the support task screen).
/// </summary>
public sealed class CommentQueryParams
{
    // ─────────────────────────────────────────────────────────────────────────
    // REQUIRED SCOPE
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// The parent request whose comments or history are being fetched.
    /// Always required — comments are never fetched across multiple requests.
    /// </summary>
    public Guid RequestId { get; init; }

    // ─────────────────────────────────────────────────────────────────────────
    // FILTERS
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Filter comments by type (maps to CommentType int values defined in CommentTypeConstants).
    /// Null returns all comment types.
    /// Example: pass CommentTypeConstants.Resolution to show only resolution notes.
    /// </summary>
    public int? CommentType { get; init; }

    /// <summary>
    /// When true, includes soft-deleted comments in the result.
    /// Should only be set by Admin-level callers for audit purposes.
    /// Defaults to false — soft-deleted comments are hidden from all normal views.
    /// </summary>
    public bool IncludeDeleted { get; init; } = false;

    // ─────────────────────────────────────────────────────────────────────────
    // SORT
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Sort direction for CreatedOn. <c>true</c> = ascending (oldest first, chronological thread).
    /// <c>false</c> = descending (newest first).
    /// Defaults to <c>true</c> — comment threads always read top to bottom chronologically.
    /// </summary>
    public bool SortAscending { get; init; } = true;
}
