using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NestHub.Infrastructure.Persistence;

/// <summary>
/// Enables `dotnet ef` design-time tooling (migrations) to run without a fully configured host.
/// The connection string here is only used for generating migrations, never at runtime.
/// </summary>
public sealed class NestHubDbContextFactory : IDesignTimeDbContextFactory<NestHubDbContext>
{
    public NestHubDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<NestHubDbContext>();
        optionsBuilder.UseSqlServer("Server=AMIT-AGRAWAL-PM;Database=NestHub;Trusted_Connection=True;TrustServerCertificate=True;");

        return new NestHubDbContext(optionsBuilder.Options);
    }
}
