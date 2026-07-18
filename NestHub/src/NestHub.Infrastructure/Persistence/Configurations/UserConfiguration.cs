using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NestHub.Domain.Common;
using NestHub.Domain.Users;

namespace NestHub.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasConversion(id => id.Value, value => new UserId(value))
            .ValueGeneratedNever();

        builder.Property(u => u.PhoneNumber)
            .HasConversion(p => p.Value, v => Domain.ValueObjects.PhoneNumber.Create(v))
            .HasColumnName("PhoneNumber")
            .HasMaxLength(20)
            .IsRequired();
        builder.HasIndex(u => u.PhoneNumber).IsUnique();

        builder.Property(u => u.Email)
            .HasConversion(
                e => e == null ? null : e.Value,
                v => v == null ? null : Domain.ValueObjects.Email.Create(v))
            .HasColumnName("Email")
            .HasMaxLength(320);

        builder.Property(u => u.PasswordHash).HasMaxLength(500).IsRequired();
        builder.Property(u => u.UserType).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(u => u.IsVerified).IsRequired();
        builder.Property(u => u.IsActive).IsRequired();
        builder.Property(u => u.CreatedDateTimeUtc).HasColumnName("CreatedDateTime").IsRequired();

        builder.Ignore(u => u.DomainEvents);
    }
}
