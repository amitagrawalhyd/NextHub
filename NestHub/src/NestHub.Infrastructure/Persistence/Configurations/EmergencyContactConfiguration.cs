using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NestHub.Domain.Common;
using NestHub.Domain.EmergencyContacts;

namespace NestHub.Infrastructure.Persistence.Configurations;

public sealed class EmergencyContactConfiguration : IEntityTypeConfiguration<EmergencyContact>
{
    public void Configure(EntityTypeBuilder<EmergencyContact> builder)
    {
        builder.ToTable("EmergencyContacts");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasConversion(id => id.Value, value => new EmergencyContactId(value))
            .ValueGeneratedNever();

        builder.Property(c => c.SocietyId).HasConversion(id => id.Value, value => new SocietyId(value)).IsRequired();
        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Role).HasMaxLength(100).IsRequired();
        builder.Property(c => c.PhoneNumber).HasMaxLength(20).IsRequired();

        builder.HasIndex(c => c.SocietyId);

        builder.HasOne<Domain.Societies.Society>().WithMany().HasForeignKey(c => c.SocietyId).OnDelete(DeleteBehavior.Cascade);
    }
}
