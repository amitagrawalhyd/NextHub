using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NestHub.Domain.Common;
using NestHub.Domain.Vendors;

namespace NestHub.Infrastructure.Persistence.Configurations;

public sealed class VendorBroadcastConfiguration : IEntityTypeConfiguration<VendorBroadcast>
{
    public void Configure(EntityTypeBuilder<VendorBroadcast> builder)
    {
        builder.ToTable("VendorBroadcasts");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .HasConversion(id => id.Value, value => new VendorBroadcastId(value))
            .ValueGeneratedNever();

        builder.Property(b => b.VendorId).HasConversion(id => id.Value, value => new VendorId(value)).IsRequired();
        builder.Property(b => b.Title).HasMaxLength(200).IsRequired();
        builder.Property(b => b.Message).HasMaxLength(2000).IsRequired();
        builder.Property(b => b.CreatedDateTimeUtc).HasColumnName("CreatedDateTime").IsRequired();
        builder.Property(b => b.ExpiresAtUtc);

        builder.HasIndex(b => b.VendorId);

        builder.HasOne<Vendor>().WithMany().HasForeignKey(b => b.VendorId).OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(b => b.IsActive);
    }
}
