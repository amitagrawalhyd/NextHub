using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NestHub.Domain.Common;
using NestHub.Domain.Reviews;
using NestHub.Domain.ValueObjects;

namespace NestHub.Infrastructure.Persistence.Configurations;

public sealed class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews", t => t.HasCheckConstraint("CK_Reviews_Rating", "[Rating] BETWEEN 1 AND 5"));
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasConversion(id => id.Value, value => new ReviewId(value))
            .ValueGeneratedNever();

        builder.Property(r => r.ResidentId).HasConversion(id => id.Value, value => new ResidentId(value)).IsRequired();
        builder.Property(r => r.VendorId).HasConversion(id => id.Value, value => new VendorId(value)).IsRequired();
        builder.Property(r => r.SocietyId).HasConversion(id => id.Value, value => new SocietyId(value)).IsRequired();

        builder.HasIndex(r => new { r.VendorId, r.SocietyId });

        builder.Property(r => r.Rating).HasConversion(rating => rating.Value, value => Rating.Create(value)).HasColumnName("Rating").IsRequired();
        builder.Property(r => r.Comment).HasMaxLength(2000);
        builder.Property(r => r.SentimentScore).HasColumnType("decimal(3,2)");
        builder.Property(r => r.IsFlagged).IsRequired();
        builder.Property(r => r.CreatedDateTimeUtc).HasColumnName("CreatedDateTime").IsRequired();

        builder.HasOne<Domain.Residents.Resident>().WithMany().HasForeignKey(r => r.ResidentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Domain.Vendors.Vendor>().WithMany().HasForeignKey(r => r.VendorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Domain.Societies.Society>().WithMany().HasForeignKey(r => r.SocietyId).OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(r => r.DomainEvents);
    }
}
