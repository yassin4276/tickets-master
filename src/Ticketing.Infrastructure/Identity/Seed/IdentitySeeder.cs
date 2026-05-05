using Microsoft.AspNetCore.Identity;

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
}