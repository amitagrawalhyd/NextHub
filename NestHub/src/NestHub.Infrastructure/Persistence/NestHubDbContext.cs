using Microsoft.EntityFrameworkCore;
using NestHub.Domain.Analytics;
using NestHub.Domain.Announcements;
using NestHub.Domain.Categories;
using NestHub.Domain.EmergencyContacts;
using NestHub.Domain.Residents;
using NestHub.Domain.Reviews;
using NestHub.Domain.Societies;
using NestHub.Domain.SosRequests;
using NestHub.Domain.Users;
using NestHub.Domain.Vendors;

namespace NestHub.Infrastructure.Persistence;

public sealed class NestHubDbContext : DbContext
{
    public NestHubDbContext(DbContextOptions<NestHubDbContext> options) : base(options)
    {
    }

    public DbSet<Society> Societies => Set<Society>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Resident> Residents => Set<Resident>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<SosRequest> SosRequests => Set<SosRequest>();
    public DbSet<SosClaim> SosClaims => Set<SosClaim>();
    public DbSet<AnalyticsLog> AnalyticsLogs => Set<AnalyticsLog>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<VendorSocietyCoverage> VendorSocietyCoverages => Set<VendorSocietyCoverage>();
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<EmergencyContact> EmergencyContacts => Set<EmergencyContact>();
    public DbSet<VendorFavorite> VendorFavorites => Set<VendorFavorite>();
    public DbSet<VendorBroadcast> VendorBroadcasts => Set<VendorBroadcast>();
    public DbSet<VendorMute> VendorMutes => Set<VendorMute>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NestHubDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
