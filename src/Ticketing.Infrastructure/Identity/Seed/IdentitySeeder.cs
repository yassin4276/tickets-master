using Microsoft.AspNetCore.Identity;
using Ticketing.Infrastructure.Identity;

namespace Ticketing.Infrastructure.Identity.Seed;

public static class IdentitySeeder
{
    public static async Task SeedRolesAsync(RoleManager<IdentityRole<int>> roleManager)
    {
        string[] roles =
        {
            "User",
            "EventOwner",
            "Admin"
        };

        foreach (var role in roles)
        {
            var exists = await roleManager.RoleExistsAsync(role);

            if (!exists)
            {
                var result = await roleManager.CreateAsync(new IdentityRole<int>(role));

                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to seed role '{role}': {errors}");
                }
            }
        }
    }

    public static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        // 3 test users for local/dev/testing
        var users = new[]
        {
            new
            {
                Email = "admin@test.local",
                FullName = "Test Admin",
                Password = "Test1234",
                Roles = new[] { "Admin" }
            },
            new
            {
                Email = "owner@test.local",
                FullName = "Test Event Owner",
                Password = "Test1234",
                Roles = new[] { "EventOwner" }
            },
            new
            {
                Email = "user@test.local",
                FullName = "Test User",
                Password = "Test1234",
                Roles = new[] { "User" }
            }
        };

        foreach (var u in users)
        {
            var existing = await userManager.FindByEmailAsync(u.Email);
            if (existing is null)
            {
                var user = new ApplicationUser
                {
                    UserName = u.Email,
                    Email = u.Email,
                    FullName = u.FullName,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(user, u.Password);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to seed user '{u.Email}': {errors}");
                }

                existing = user;
            }

            foreach (var role in u.Roles)
            {
                if (!await userManager.IsInRoleAsync(existing, role))
                {
                    var addRoleResult = await userManager.AddToRoleAsync(existing, role);
                    if (!addRoleResult.Succeeded)
                    {
                        var errors = string.Join("; ", addRoleResult.Errors.Select(e => e.Description));
                        throw new InvalidOperationException($"Failed to add role '{role}' to '{u.Email}': {errors}");
                    }
                }
            }
        }
    }
}