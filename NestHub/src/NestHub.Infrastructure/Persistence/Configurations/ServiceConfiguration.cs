using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NestHub.Domain.Common;
using NestHub.Domain.Enums;
using NestHub.Domain.ValueObjects;
using NestHub.Domain.Vendors;

namespace NestHub.Infrastructure.Persistence.Configurations;

public sealed class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    private sealed record PricingInfoJson(PricingType Type, decimal? Amount, string Currency);

    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("Services");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasConversion(id => id.Value, value => new ServiceId(value))
            .ValueGeneratedNever();

        builder.Property(s => s.VendorId).HasConversion(id => id.Value, value => new VendorId(value)).IsRequired();
        builder.HasIndex(s => s.VendorId);

        builder.Property(s => s.Title).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Description).HasMaxLength(2000);
        builder.Property(s => s.Category).HasMaxLength(100).IsRequired();
        builder.HasIndex(s => s.Category);

        builder.Property(s => s.Pricing)
            .HasConversion(
                p => JsonSerializer.Serialize(new PricingInfoJson(p.Type, p.Amount, p.Currency), (JsonSerializerOptions?)null),
                json => ToPricingInfo(JsonSerializer.Deserialize<PricingInfoJson>(json, (JsonSerializerOptions?)null)!))
            .HasColumnName("PricingJson");
    }

    private static PricingInfo ToPricingInfo(PricingInfoJson json) => json.Type switch
    {
        PricingType.Fixed => PricingInfo.Fixed(json.Amount!.Value, json.Currency),
        PricingType.Hourly => PricingInfo.Hourly(json.Amount!.Value, json.Currency),
        PricingType.StartingFrom => PricingInfo.StartingFrom(json.Amount!.Value, json.Currency),
        _ => PricingInfo.ContactForQuote()
    };
}
