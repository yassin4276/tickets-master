using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ticketing.Application.Interfaces.Email;
using Ticketing.Infrastructure.Persistence;

namespace Ticketing.IntegrationTests.Common;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection;

    public CustomWebApplicationFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(
            [
                new KeyValuePair<string, string?>(
                    "JwtSettings:SecretKey",
                    "IntegrationTests-JwtSigningKey-Min32Chars!!"),
                new KeyValuePair<string, string?>("JwtSettings:Issuer", "TicketingAPI"),
                new KeyValuePair<string, string?>("JwtSettings:Audience", "TicketingAPIUsers"),
                new KeyValuePair<string, string?>("JwtSettings:ExpirationMinutes", "60"),
                new KeyValuePair<string, string?>("JwtSettings:RefreshTokenAudience", "TicketingAPIRefresh"),
                new KeyValuePair<string, string?>("JwtSettings:RefreshTokenExpiryDays", "7"),
            ]);
        });

        builder.ConfigureTestServices(services =>
        {
            // Remove all ApplicationDbContext / EF Core registrations from the real app
            services.RemoveAll<ApplicationDbContext>();
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<DbContextOptions>();

            var dbContextDescriptors = services
                .Where(d =>
                    d.ServiceType.FullName != null &&
                    (
                        d.ServiceType.FullName.Contains("ApplicationDbContext") ||
                        d.ServiceType.FullName.Contains("DbContextOptions")
                    ))
                .ToList();

            foreach (var descriptor in dbContextDescriptors)
            {
                services.Remove(descriptor);
            }

            // Replace real email service with fake one
            services.RemoveAll<IEmailService>();
            services.AddScoped<IEmailService, FakeEmailService>();

            // Add SQLite in-memory database for tests
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();

            SeedRolesAsync(scope.ServiceProvider).GetAwaiter().GetResult();
        });
    }

    private static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

        var roles = new[] { "User", "EventOwner", "Admin" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<int>(role));
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection.Dispose();
        }
    }
}