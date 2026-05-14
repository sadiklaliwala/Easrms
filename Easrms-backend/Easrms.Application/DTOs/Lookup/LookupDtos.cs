// Easrms.Application/DTOs/Lookup/LookupDtos.cs

namespace Easrms.Application.DTOs.Lookup;

/// <summary>
/// Flat dropdown DTO for active support users.
/// Used by GET /api/lookup/support-users — feeds the assign-request dropdown.
/// Only UserId and FullName exposed — no role, no email, no sensitive data.
/// </summary>
public sealed class SupportUserLookupDto
{
    /// <summary>Unique identifier of the support user. Sent as AssignRequestDto.SupportUserId.</summary>
    public Guid UserId { get; init; }

    /// <summary>Display name shown in the dropdown.</summary>
    public string FullName { get; init; } = string.Empty;
}

/// <summary>
/// Flat dropdown DTO for active managers.
/// Used by GET /api/lookup/managers — feeds the manager dropdown on Create/Edit User screens.
/// Only UserId and FullName exposed.
/// </summary>
public sealed class ManagerLookupDto
{
    /// <summary>Unique identifier of the manager. Sent as CreateUserDto.ManagerId or UpdateUserDto.ManagerId.</summary>
    public Guid UserId { get; init; }

    /// <summary>Display name shown in the dropdown.</summary>
    public string FullName { get; init; } = string.Empty;
}

/// <summary>
/// Flat dropdown DTO for active request categories.
/// Used by the Create Request screen category dropdown.
/// Includes IsApprovalRequired so the frontend can show a visual indicator
/// (e.g. "Requires Approval" badge) next to the category name before submission.
/// </summary>
public sealed class CategoryLookupDto
{
    /// <summary>Unique identifier of the category. Sent as CreateRequestDto.CategoryId.</summary>
    public Guid CategoryId { get; init; }

    /// <summary>Display name shown in the dropdown.</summary>
    public string CategoryName { get; init; } = string.Empty;

    /// <summary>
    /// Whether requests under this category require manager approval before assignment.
    /// Frontend uses this to display an approval warning on the Create Request form
    /// when the user selects this category.
    /// </summary>
    public bool IsApprovalRequired { get; init; }
}
