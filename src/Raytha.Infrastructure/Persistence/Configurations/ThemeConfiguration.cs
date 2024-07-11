using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raytha.Domain.Entities;

namespace Raytha.Infrastructure.Persistence.Configurations;

public class ThemeConfiguration : IEntityTypeConfiguration<Theme>
{
    public void Configure(EntityTypeBuilder<Theme> builder)
    {
        builder
            .HasIndex(t => t.DeveloperName)
            .IsUnique();

        builder
            .HasOne(t => t.CreatorUser)
            .WithMany()
            .HasForeignKey(t => t.CreatorUserId);

        builder
            .HasOne(t => t.LastModifierUser)
            .WithMany()
            .HasForeignKey(t => t.LastModifierUserId);
    }
}