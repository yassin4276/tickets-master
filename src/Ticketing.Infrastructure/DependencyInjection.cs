using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ticketing.Application;
using Ticketing.Application.Interfaces.Booking;
using Ticketing.Application.Interfaces.Email;
using Ticketing.Application.Interfaces.EventOwner;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Application.Interfaces.User;
using Ticketing.Infrastructure.Auth;
using Ticketing.Infrastructure.Booking;
using Ticketing.Infrastructure.Email;
using Ticketing.Infrastructure.EventOwner;
using Ticketing.Infrastructure.Identity;
using Ticketing.Infrastructure.Persistence;
using Ticketing.Infrastructure.User;

namespace Ticketing.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;

                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole<int>>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
                
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<JwtSettings>>().Value);

        services.Configure<AppUrlSettings>(configuration.GetSection("AppUrls"));
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<AppUrlSettings>>().Value);

        services.Configure<EmailSettings>(
            configuration.GetSection("EmailSettings"));
        
        services.AddSignalR();

        services.AddScoped<IEmailService, EmailService>();

        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        services.AddScoped<IEventOwnerService, EventOwnerService>();

        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IBookingService, BookingService>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}