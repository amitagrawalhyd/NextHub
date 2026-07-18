using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NestHub.Domain.Common;
using NestHub.Domain.SosRequests;

namespace NestHub.Infrastructure.Persistence.Configurations;

public sealed class SosRequestConfiguration : IEntityTypeConfiguration<SosRequest>
{
    public void Configure(EntityTypeBuilder<SosRequest> builder)
    {
        builder.ToTable("SosRequests");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasConversion(id => id.Value, value => new SosRequestId(value))
            .ValueGeneratedNever();

        builder.Property(r => r.ResidentId).HasConversion(id => id.Value, value => new ResidentId(value)).IsRequired();
        builder.Property(r => r.SocietyId).HasConversion(id => id.Value, value => new SocietyId(value)).IsRequired();
        builder.HasIndex(r => new { r.SocietyId, r.Category, r.Status });

        builder.Property(r => r.Category).HasMaxLength(100).IsRequired();
        builder.Property(r => r.Description).HasMaxLength(2000).IsRequired();
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(r => r.CreatedDateTimeUtc).HasColumnName("CreatedDateTime").IsRequired();

        builder.HasOne<Domain.Residents.Resident>().WithMany().HasForeignKey(r => r.ResidentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Domain.Societies.Society>().WithMany().HasForeignKey(r => r.SocietyId).OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.Claims)
            .WithOne()
            .HasForeignKey(c => c.SosRequestId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(r => r.Claims).UsePropertyAccessMode(PropertyAccessMode.Field).HasField("_claims");

        builder.Ignore(r => r.DomainEvents);
    }
}
