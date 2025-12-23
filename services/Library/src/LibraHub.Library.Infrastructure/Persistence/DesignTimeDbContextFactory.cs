using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LibraHub.Library.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<LibraryDbContext>
{
    public LibraryDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LibraryDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=librahub_library;Username=librahub_admin;Password=L1br@Hub_DB_2026!S3cur3_P@ss");

        return new LibraryDbContext(optionsBuilder.Options);
    }
}

