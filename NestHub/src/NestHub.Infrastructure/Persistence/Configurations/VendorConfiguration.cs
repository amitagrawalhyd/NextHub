using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NestHub.Domain.Common;
using NestHub.Domain.Vendors;
using NestHub.Domain.ValueObjects;

namespace NestHub.Infrastructure.Persistence.Configurations;

public sealed class VendorConfiguration : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> builder)
    {
        builder.ToTable("Vendors");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id)
            .HasConversion(id => id.Value, value => new VendorId(value))
            .ValueGeneratedNever();

        builder.Property(v => v.UserId).HasConversion(id => id.Value, value => new UserId(value)).IsRequired();
        builder.HasIndex(v => v.UserId).IsUnique();
        builder.HasOne<Domain.Users.User>().WithMany().HasForeignKey(v => v.UserId).OnDelete(DeleteBehavior.Cascade);

        builder.Property(v => v.BusinessName).HasMaxLength(200).IsRequired();
        builder.Property(v => v.LogoUrl).HasMaxLength(1000);
        builder.Property(v => v.Bio).HasMaxLength(2000);

        builder.Property(v => v.WhatsAppNumber)
            .HasConversion(p => p.Value, v => PhoneNumber.Create(v))
            .HasColumnName("WhatsAppNumber")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(v => v.OperatingHours)
            .HasConversion(
                oh => JsonSerializer.Serialize(oh.Days, (JsonSerializerOptions?)null),
                json => OperatingHours.Create(JsonSerializer.Deserialize<Dictionary<DayOfWeek, DailyHours>>(json, (JsonSerializerOptions?)null)!))
            .HasColumnName("OperatingHoursJson");

        builder.Property(v => v.SubscriptionTier).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(v => v.TrustBadgeStatus).HasConversion<string>().HasColumnName("TrustBadgeStatus").HasMaxLength(30).IsRequired();
        builder.Property(v => v.AverageRating).HasColumnType("decimal(3,2)").IsRequired();
        builder.Property(v => v.IsApproved).IsRequired();

        builder.HasMany(v => v.Services)
            .WithOne()
            .HasForeignKey(s => s.VendorId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(v => v.Services).UsePropertyAccessMode(PropertyAccessMode.Field).HasField("_services");

        builder.Ignore(v => v.DomainEvents);
    }
}
