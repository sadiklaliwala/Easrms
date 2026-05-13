namespace Easrms.Domain.Entities;

public class Role
{
    public Guid RoleId { get; set; }

    public string RoleName { get; set; } = string.Empty;


    // Navigation Properties
    public ICollection<User> Users { get; set; } = new List<User>();
}

