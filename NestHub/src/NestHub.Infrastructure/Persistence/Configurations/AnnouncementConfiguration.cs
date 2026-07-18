using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NestHub.Domain.Announcements;
using NestHub.Domain.Common;

namespace NestHub.Infrastructure.Persistence.Configurations;

public sealed class AnnouncementConfiguration : IEntityTypeConfiguration<Announcement>
{
    public void Configure(EntityTypeBuilder<Announcement> builder)
    {
        builder.ToTable("Announcements");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasConversion(id => id.Value, value => new AnnouncementId(value))
            .ValueGeneratedNever();

        builder.Property(a => a.SocietyId).HasConversion(id => id.Value, value => new SocietyId(value)).IsRequired();
        builder.Property(a => a.Title).HasMaxLength(200).IsRequired();
        builder.Property(a => a.Body).HasMaxLength(2000).IsRequired();
        builder.Property(a => a.CreatedDateTimeUtc).HasColumnName("CreatedDateTime").IsRequired();
        builder.Property(a => a.ExpiresAtUtc);

        builder.HasIndex(a => a.SocietyId);

        builder.HasOne<Domain.Societies.Society>().WithMany().HasForeignKey(a => a.SocietyId).OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(a => a.IsActive);
    }
}
