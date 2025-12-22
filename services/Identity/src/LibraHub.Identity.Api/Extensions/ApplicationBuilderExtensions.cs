using LibraHub.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LibraHub.Identity.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseIdentityDatabaseMigrations(this IApplicationBuilder app)
    {
        if (app.ApplicationServices.GetRequiredService<Microsoft.Extensions.Hosting.IHostEnvironment>().IsDevelopment())
        {
            using var scope = app.ApplicationServices.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
            dbContext.Database.Migrate();
        }

        return app;
    }

    public static IApplicationBuilder UseIdentityDatabaseSeeder(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        seeder.SeedAsync().GetAwaiter().GetResult();

        return app;
    }
}
