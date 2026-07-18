using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NestHub.Domain.Common;
using NestHub.Domain.Residents;

namespace NestHub.Infrastructure.Persistence.Configurations;

public sealed class ResidentConfiguration : IEntityTypeConfiguration<Resident>
{
    public void Configure(EntityTypeBuilder<Resident> builder)
    {
        builder.ToTable("Residents");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasConversion(id => id.Value, value => new ResidentId(value))
            .ValueGeneratedNever();

        builder.Property(r => r.UserId).HasConversion(id => id.Value, value => new UserId(value)).IsRequired();
        builder.Property(r => r.SocietyId).HasConversion(id => id.Value, value => new SocietyId(value)).IsRequired();
        builder.Property(r => r.BlockNumber).HasMaxLength(50).IsRequired();
        builder.Property(r => r.FlatNumber).HasMaxLength(50).IsRequired();
        builder.Property(r => r.Name).HasMaxLength(200).IsRequired();

        builder.HasIndex(r => r.UserId).IsUnique();
        builder.HasIndex(r => r.SocietyId);

        builder.HasOne<Domain.Users.User>().WithMany().HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Domain.Societies.Society>().WithMany().HasForeignKey(r => r.SocietyId).OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(r => r.DomainEvents);
    }
}
