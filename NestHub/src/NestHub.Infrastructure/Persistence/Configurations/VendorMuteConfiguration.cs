using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NestHub.Domain.Common;
using NestHub.Domain.Vendors;

namespace NestHub.Infrastructure.Persistence.Configurations;

public sealed class VendorMuteConfiguration : IEntityTypeConfiguration<VendorMute>
{
    public void Configure(EntityTypeBuilder<VendorMute> builder)
    {
        builder.ToTable("VendorMutes");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasConversion(id => id.Value, value => new VendorMuteId(value))
            .ValueGeneratedNever();

        builder.Property(m => m.ResidentId).HasConversion(id => id.Value, value => new ResidentId(value)).IsRequired();
        builder.Property(m => m.VendorId).HasConversion(id => id.Value, value => new VendorId(value)).IsRequired();
        builder.Property(m => m.CreatedDateTimeUtc).HasColumnName("CreatedDateTime").IsRequired();

        builder.HasIndex(m => new { m.ResidentId, m.VendorId }).IsUnique();

        builder.HasOne<Domain.Residents.Resident>().WithMany().HasForeignKey(m => m.ResidentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Vendor>().WithMany().HasForeignKey(m => m.VendorId).OnDelete(DeleteBehavior.Restrict);
    }
}
