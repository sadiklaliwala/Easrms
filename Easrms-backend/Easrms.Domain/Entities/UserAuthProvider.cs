using Easrms.Common.Constants;

namespace Easrms.Domain.Entities;

public class UserAuthProvider
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public AuthProviderEnum AuthProvider { get; set; }
    public string? ExternalUserId { get; set; }
    public DateTime CreatedOn { get; set; }

    // Navigation
    public User User { get; set; } = null!;
}
