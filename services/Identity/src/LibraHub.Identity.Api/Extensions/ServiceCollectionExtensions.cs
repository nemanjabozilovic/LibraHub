using FluentValidation;
using LibraHub.BuildingBlocks.Messaging;
using LibraHub.Identity.Application;
using LibraHub.Identity.Application.Abstractions;
using LibraHub.Identity.Infrastructure.CurrentUser;
using LibraHub.Identity.Infrastructure.Messaging;
using LibraHub.Identity.Infrastructure.Options;
using LibraHub.Identity.Infrastructure.Persistence;
using LibraHub.Identity.Infrastructure.Repositories;
using LibraHub.Identity.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace LibraHub.Identity.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("IdentityDb")
            ?? throw new InvalidOperationException("Connection string 'IdentityDb' not found.");

        services.AddDbContext<IdentityDbContext>(options =>
            options.UseNpgsql(connectionString));

        return services;
    }

    public static IServiceCollection AddIdentityApplicationServices(this IServiceCollection services)
    {
        // MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ApplicationAssembly).Assembly));

        // FluentValidation
        services.AddValidatorsFromAssembly(typeof(ApplicationAssembly).Assembly);

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IEmailVerificationTokenRepository, EmailVerificationTokenRepository>();

        // Security services
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IEmailVerificationTokenService, EmailVerificationTokenService>();

        // Infrastructure services
        services.AddScoped<BuildingBlocks.Abstractions.IOutboxWriter, IdentityEventPublisher>();
        services.AddScoped<BuildingBlocks.Abstractions.IClock, BuildingBlocks.Clock>();
        services.AddHttpContextAccessor();
        services.AddScoped<BuildingBlocks.Abstractions.ICurrentUser, CurrentUser>();

        // Database seeder
        services.AddScoped<DatabaseSeeder>();

        return services;
    }

    public static IServiceCollection AddIdentityJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtOptions = configuration.GetSection("Jwt").Get<JwtOptions>()
            ?? throw new InvalidOperationException("JWT options not configured");

        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.Configure<SecurityOptions>(configuration.GetSection("Security"));

        services.AddAuthentication(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                        System.Text.Encoding.UTF8.GetBytes(jwtOptions.SecretKey))
                };
            });

        services.AddAuthorization();

        return services;
    }

    public static IServiceCollection AddIdentityRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitMqConnectionString = configuration.GetConnectionString("RabbitMQ")
            ?? throw new InvalidOperationException("RabbitMQ connection string not configured");

        var rabbitMqConnection = RabbitMqSetup.CreateConnection(rabbitMqConnectionString);
        services.AddSingleton(rabbitMqConnection);
        services.AddHostedService<IdentityOutboxPublisherWorker>();

        return services;
    }

    public static IServiceCollection AddIdentityHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("IdentityDb")
            ?? throw new InvalidOperationException("Connection string 'IdentityDb' not found.");

        services.AddHealthChecks()
            .AddNpgSql(connectionString, name: "database");

        return services;
    }
}
