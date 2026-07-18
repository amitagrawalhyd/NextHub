using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NestHub.Domain.Common;
using NestHub.Domain.SosRequests;

namespace NestHub.Infrastructure.Persistence.Configurations;

public sealed class SosClaimConfiguration : IEntityTypeConfiguration<SosClaim>
{
    public void Configure(EntityTypeBuilder<SosClaim> builder)
    {
        builder.ToTable("SosClaims");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasConversion(id => id.Value, value => new SosClaimId(value))
            .ValueGeneratedNever();

        builder.Property(c => c.SosRequestId).HasConversion(id => id.Value, value => new SosRequestId(value)).IsRequired();
        builder.Property(c => c.VendorId).HasConversion(id => id.Value, value => new VendorId(value)).IsRequired();
        builder.Property(c => c.ClaimedDateTimeUtc).HasColumnName("ClaimedDateTime").IsRequired();

        builder.HasIndex(c => new { c.SosRequestId, c.VendorId }).IsUnique();

        builder.HasOne<Domain.Vendors.Vendor>().WithMany().HasForeignKey(c => c.VendorId).OnDelete(DeleteBehavior.Restrict);
    }
}
