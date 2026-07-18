using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NestHub.Domain.Societies;
using NestHub.Domain.ValueObjects;

namespace NestHub.Infrastructure.Persistence.Configurations;

public sealed class SocietyConfiguration : IEntityTypeConfiguration<Society>
{
    public void Configure(EntityTypeBuilder<Society> builder)
    {
        builder.ToTable("Societies");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasConversion(id => id.Value, value => new NestHub.Domain.Common.SocietyId(value))
            .ValueGeneratedNever();

        builder.Property(s => s.Name).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Address).HasMaxLength(500).IsRequired();
        builder.Property(s => s.City).HasMaxLength(100).IsRequired().HasDefaultValue("Hyderabad");

        builder.Property(s => s.GeoLocation)
            .HasConversion(
                geo => geo == null ? null : $"{geo.Latitude},{geo.Longitude}",
                value => value == null ? null : ParseGeoLocation(value))
            .HasColumnName("GeoLocation")
            .HasMaxLength(64);

        builder.Property(s => s.IsActive).IsRequired();
        builder.Property(s => s.CreatedDateTimeUtc).HasColumnName("CreatedDateTime").IsRequired();

        builder.Ignore(s => s.DomainEvents);
    }

    private static GeoLocation? ParseGeoLocation(string value)
    {
        var parts = value.Split(',');
        return GeoLocation.Create(double.Parse(parts[0]), double.Parse(parts[1]));
    }
}
