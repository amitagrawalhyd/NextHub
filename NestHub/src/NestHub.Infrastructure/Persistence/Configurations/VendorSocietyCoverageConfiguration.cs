using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NestHub.Domain.Common;
using NestHub.Domain.Enums;
using NestHub.Domain.Vendors;

namespace NestHub.Infrastructure.Persistence.Configurations;

public sealed class VendorSocietyCoverageConfiguration : IEntityTypeConfiguration<VendorSocietyCoverage>
{
    public void Configure(EntityTypeBuilder<VendorSocietyCoverage> builder)
    {
        builder.ToTable("VendorSocietyCoverages");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasConversion(id => id.Value, value => new VendorSocietyCoverageId(value))
            .ValueGeneratedNever();

        builder.Property(c => c.VendorId).HasConversion(id => id.Value, value => new VendorId(value)).IsRequired();
        builder.Property(c => c.SocietyId).HasConversion(id => id.Value, value => new SocietyId(value)).IsRequired();
        builder.Property(c => c.AffiliationType).HasConversion<string>().HasMaxLength(20).IsRequired().HasDefaultValue(AffiliationType.Manual);

        builder.HasIndex(c => new { c.VendorId, c.SocietyId }).IsUnique();

        // At most one InHouse row per vendor (a vendor has at most one "home" society).
        // Manual/Nearby rows are unrestricted in count per vendor.
        builder.HasIndex(c => c.VendorId)
            .IsUnique()
            .HasFilter("[AffiliationType] = 'InHouse'")
            .HasDatabaseName("UX_VendorSocietyCoverages_Vendor_InHouse");

        builder.HasOne<Vendor>().WithMany().HasForeignKey(c => c.VendorId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Domain.Societies.Society>().WithMany().HasForeignKey(c => c.SocietyId).OnDelete(DeleteBehavior.Restrict);
    }
}
