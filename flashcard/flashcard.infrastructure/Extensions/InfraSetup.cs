using flashcard.application.ServiceContracts;
using flashcard.infrastructure.DbContext;
using flashcard.infrastructure.IdentityEntities;
using flashcard.infrastructure.Services;
using flashcard.infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace flashcard.infrastructure.Extensions;

public static class InfraSetup
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        var settingSection = configuration.GetSection("JwtSettings");
        services.Configure<JwtSettings>(settingSection);
        services.AddIdentity<User, Role>(options =>
        {
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 4;
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequiredUniqueChars = 0;
        })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders()
            .AddUserStore<UserStore<User, Role, AppDbContext, Guid>>()
            .AddRoleStore<RoleStore<Role, AppDbContext, Guid>>();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }
        )
            .AddJwtBearer(options =>
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidIssuer = settingSection.GetValue<string>("Issuer"),
                ValidateAudience = true,
                ValidAudience = settingSection.GetValue<string>("Audience"),
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settingSection.GetValue<string>("Key")))
            });

        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();

            options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();

            options.AddPolicy("onlyadmin", pb => pb.RequireRole("Admin"));
        }
        );

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("Default"))
        );

        services.AddScoped<ITokenGenerator, TokenGenerator>();
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }

    public static async Task SeedData(IServiceProvider rootProvider)
    {
        using var scope = rootProvider.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();

        string[] names = { "Admin", "User" };
        foreach (var name in names)
        {
            if (!await roleManager.RoleExistsAsync(name))
            {
                await roleManager.CreateAsync(new Role() { Name = name });
            }
        }

        foreach (var name in names)
        {
            var user = await userManager.FindByNameAsync(name);
            if (user == null)
            {
                user = new User() { UserName = name };
                await userManager.CreateAsync(user, name);
                await userManager.AddToRoleAsync(user, name);
            }
            else if (!await userManager.IsInRoleAsync(user, name))
            {
                await userManager.AddToRoleAsync(user, name);
            }
        }
    }
}
