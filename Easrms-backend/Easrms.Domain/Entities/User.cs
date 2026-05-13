namespace Easrms.Domain.Entities;

public class User
{
    public Guid UserId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public Guid RoleId { get; set; }

    public Guid? ManagerId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public DateTime? LastLoginOn { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiryOn { get; set; }


    // Navigation Properties
    public Role Role { get; set; } = null!;

    public User? Manager { get; set; }

    public ICollection<User> Employees { get; set; } = new List<User>();

    public ICollection<ServiceRequest> CreatedRequests { get; set; } = new List<ServiceRequest>();

    public ICollection<ServiceRequest> AssignedRequests { get; set; } = new List<ServiceRequest>();

    public ICollection<ServiceRequest> ClosedRequests { get; set; } = new List<ServiceRequest>();

    public ICollection<RequestComment> Comments { get; set; } = new List<RequestComment>();

    public ICollection<RequestStatusHistory> StatusHistories { get; set; } = new List<RequestStatusHistory>();
}

