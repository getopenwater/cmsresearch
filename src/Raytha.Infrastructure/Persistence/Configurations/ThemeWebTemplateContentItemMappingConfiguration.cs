using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raytha.Domain.Entities;

namespace Raytha.Infrastructure.Persistence.Configurations;

public class ThemeWebTemplateContentItemMappingConfiguration : IEntityTypeConfiguration<ThemeWebTemplateContentItemMapping>
{
    public void Configure(EntityTypeBuilder<ThemeWebTemplateContentItemMapping> builder)
    {
        builder
            .HasOne(wtm => wtm.WebTemplate)
            .WithMany()
            .HasForeignKey(wtm => wtm.WebTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(wtm => wtm.ContentItem)
            .WithMany()
            .HasForeignKey(wtm => wtm.ContentItemId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(wtm => new { wtm.ThemeId, wtm.WebTemplateId, wtm.ContentItemId })
            .IsUnique();
    }
}