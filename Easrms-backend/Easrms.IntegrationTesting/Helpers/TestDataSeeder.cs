using Easrms.Infrastructure.Data;
using Easrms.Domain.Entities;

namespace Easrms.IntegrationTesting.Helpers;

public static class TestDataSeeder
{
    public static void Seed(AppDbContext db)
    {
        // Minimal seed: roles and one user
        if (!db.Roles.Any())
        {
            var adminRole = new Role { RoleId = Guid.Parse("90a5eb56-0ddc-4187-9f32-8870f8fc7043"), RoleName = "Admin" };
            var managerRole = new Role { RoleId = Guid.Parse("90a5eb56-0ddc-4187-9f32-8870f8fc7044"), RoleName = "Manager" };
            var employeeRole = new Role { RoleId = Guid.Parse("90a5eb56-0ddc-4187-9f32-8870f8fc7045"), RoleName = "Employee" };
            var supportRole = new Role { RoleId = Guid.Parse("90a5eb56-0ddc-4187-9f32-8870f8fc7046"), RoleName = "Support" };

            db.Roles.AddRange(adminRole, managerRole, employeeRole, supportRole);
            db.SaveChanges();
        }

        if (!db.Users.Any())
        {
            var admin = new User
            {
                UserId = Guid.Parse("A1000000-0000-0000-0000-000000000001"),
                FullName = "Arjun Mehta",
                Email = "arjun.mehta@easrms.com",
                PasswordHash = Easrms.Common.Helpers.PasswordHelper.Hash("123456"),
                RoleId = Guid.Parse("90A5EB56-0DDC-4187-9F32-8870F8FC7043"),
                IsActive = true,
                CreatedOn = DateTime.UtcNow
            };

            db.Users.Add(admin);
            db.SaveChanges();
        }

        // Seed a default category for request creation
        if (!db.RequestCategories.Any())
        {
            var cat = new RequestCategory
            {
                CategoryId = Guid.Parse("E1000000-0000-0000-0000-000000000016"),
                CategoryName = "Hardware Request",
                IsActive = true,
                IsApprovalRequired = false,
                SLAHours = 24,
                CreatedOn = DateTime.UtcNow
            };
            db.RequestCategories.Add(cat);
            db.SaveChanges();
        }
    }
}
