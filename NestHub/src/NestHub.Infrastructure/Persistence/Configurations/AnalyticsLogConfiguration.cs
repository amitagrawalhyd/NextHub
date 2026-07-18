using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NestHub.Domain.Analytics;
using NestHub.Domain.Common;

namespace NestHub.Infrastructure.Persistence.Configurations;

public sealed class AnalyticsLogConfiguration : IEntityTypeConfiguration<AnalyticsLog>
{
    public void Configure(EntityTypeBuilder<AnalyticsLog> builder)
    {
        builder.ToTable("AnalyticsLogs");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .HasConversion(id => id.Value, value => new AnalyticsLogId(value))
            .ValueGeneratedNever();

        builder.Property(l => l.VendorId).HasConversion(id => id.Value, value => new VendorId(value)).IsRequired();
        builder.Property(l => l.ActionType).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(l => l.CreatedDateTimeUtc).HasColumnName("CreatedDateTime").IsRequired();

        builder.HasIndex(l => new { l.VendorId, l.CreatedDateTimeUtc });

        builder.HasOne<Domain.Vendors.Vendor>().WithMany().HasForeignKey(l => l.VendorId).OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(l => l.DomainEvents);
    }
}
