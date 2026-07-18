using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NestHub.Domain.Common;
using NestHub.Domain.Vendors;

namespace NestHub.Infrastructure.Persistence.Configurations;

public sealed class VendorFavoriteConfiguration : IEntityTypeConfiguration<VendorFavorite>
{
    public void Configure(EntityTypeBuilder<VendorFavorite> builder)
    {
        builder.ToTable("VendorFavorites");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .HasConversion(id => id.Value, value => new VendorFavoriteId(value))
            .ValueGeneratedNever();

        builder.Property(f => f.ResidentId).HasConversion(id => id.Value, value => new ResidentId(value)).IsRequired();
        builder.Property(f => f.VendorId).HasConversion(id => id.Value, value => new VendorId(value)).IsRequired();
        builder.Property(f => f.CreatedDateTimeUtc).HasColumnName("CreatedDateTime").IsRequired();

        builder.HasIndex(f => new { f.ResidentId, f.VendorId }).IsUnique();

        builder.HasOne<Domain.Residents.Resident>().WithMany().HasForeignKey(f => f.ResidentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Vendor>().WithMany().HasForeignKey(f => f.VendorId).OnDelete(DeleteBehavior.Restrict);
    }
}
