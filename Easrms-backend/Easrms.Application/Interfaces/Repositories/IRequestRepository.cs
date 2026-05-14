using Easrms.Application.DTOs.Request;
using Easrms.Domain.Entities;

namespace Easrms.Application.Interfaces.Repositories;

/// <summary>
/// Repository contract for all ServiceRequest data operations.
/// Read operations that involve filtering, joins, or aggregation use Dapper.
/// Write operations and simple single-record fetches use EF Core.
/// All methods are async and accept a <see cref="CancellationToken"/> for request cancellation support.
/// </summary>
public interface IRequestRepository
{
    // ─────────────────────────────────────────────────────────────────────────
    // QUERY METHODS
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns a paginated, filtered list of service requests using Dapper for performance.
    /// Results are role-scoped at the handler level before this call.
    /// Supports search by RequestNumber / Title, filter by Status, Priority, CategoryId,
    /// AssignedTo, EmployeeId, and date range. Sorted by CreatedOn DESC by default.
    /// </summary>
    /// <param name="queryParams">
    /// Encapsulates all filter, search, sort, and pagination inputs in one strongly-typed object.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>
    /// A <see cref="RequestListWithPaginationDto"/> containing the current page of items
    /// and full pagination metadata (TotalCount, TotalPages, PageNumber, PageSize).
    /// </returns>
    Task<RequestListWithPaginationDto> GetPagedRequestsAsync(
        RequestQueryParams queryParams,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the full detail of a single request including all navigation properties
    /// (Employee, Category, AssignedUser, ClosedByUser).
    /// Uses EF Core with explicit .Include() calls.
    /// Returns null if the request does not exist.
    /// </summary>
    /// <param name="requestId">The unique identifier of the request.</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>
    /// The <see cref="ServiceRequest"/> entity with navigations loaded, or null if not found.
    /// </returns>
    Task<ServiceRequest?> GetRequestByIdAsync(
        Guid requestId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a lightweight request entity with only the Category navigation loaded.
    /// Used specifically during request creation to check <c>IsApprovalRequired</c>
    /// without loading unnecessary navigations.
    /// Returns null if the request does not exist.
    /// </summary>
    /// <param name="requestId">The unique identifier of the request.</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>
    /// The <see cref="ServiceRequest"/> entity with Category loaded, or null if not found.
    /// </returns>
    Task<ServiceRequest?> GetRequestWithCategoryAsync(
        Guid requestId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a request with the given ID exists in the database.
    /// Lightweight — executes a single SQL EXISTS check via EF Core.
    /// Used as a guard in all command handlers before performing any mutation.
    /// </summary>
    /// <param name="requestId">The unique identifier of the request.</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns><c>true</c> if the request exists; otherwise <c>false</c>.</returns>
    Task<bool> ExistsAsync(
        Guid requestId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a generated request number already exists in the database.
    /// Called by <c>RequestNumberHelper</c> during request creation to guarantee uniqueness
    /// before persisting, avoiding a unique constraint violation at the DB level.
    /// </summary>
    /// <param name="requestNumber">The generated request number string (e.g. REQ-20260511-0042).</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns><c>true</c> if the request number is already taken; otherwise <c>false</c>.</returns>
    Task<bool> IsRequestNumberExistsAsync(
        string requestNumber,
        CancellationToken cancellationToken = default);

    // ─────────────────────────────────────────────────────────────────────────
    // COMMAND METHODS
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Inserts a new <see cref="ServiceRequest"/> record into the database.
    /// The entity must have RequestNumber, EmployeeId, CategoryId, Status,
    /// Priority, Title, and Description already set by the command handler.
    /// Does not call SaveChanges — the Unit of Work / DbContext.SaveChangesAsync
    /// is called by the handler after this method.
    /// </summary>
    /// <param name="request">The fully constructed <see cref="ServiceRequest"/> entity to insert.</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    Task AddAsync(
        ServiceRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a <see cref="ServiceRequest"/> entity as modified in the EF Core change tracker.
    /// Used by all mutation flows: Approve, Reject, Assign, UpdateStatus (InProgress / Resolved),
    /// and Close. The command handler fetches the entity, applies changes to its properties,
    /// then calls this method before SaveChangesAsync.
    /// No DB round-trip — EF Core tracks the changes on the already-fetched entity.
    /// </summary>
    /// <param name="request">The modified <see cref="ServiceRequest"/> entity.</param>
    void Update(ServiceRequest request);

    /// <summary>
    /// Persists all pending changes in the current DbContext to the database.
    /// Called once at the end of each command handler to commit the unit of work.
    /// Returns the number of state entries written to the database.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>The number of rows affected.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}