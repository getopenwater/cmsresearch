using Microsoft.EntityFrameworkCore;
using Raytha.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Raytha.Infrastructure.Persistence.Configurations;

public class ThemeWebTemplatesMappingConfiguration : IEntityTypeConfiguration<ThemeWebTemplatesMapping>
{
    public void Configure(EntityTypeBuilder<ThemeWebTemplatesMapping> builder)
    {
        builder
            .HasOne(t => t.WebTemplate)
            .WithMany()
            .HasForeignKey(t => t.WebTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(t => t.Theme)
            .WithMany(t => t.WebTemplatesMappings)
            .HasForeignKey(t => t.ThemeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(t => t.ContentItem)
            .WithMany()
            .HasForeignKey(t => t.ContentItemId);

        builder
            .HasOne(t => t.View)
            .WithMany()
            .HasForeignKey(t => t.ViewId);

        builder.ToTable(t => t.HasCheckConstraint("CK_ThemeWebTemplatesMapping_ContentItemId_ViewId", "[ContentItemId] IS NOT NULL OR [ViewId] IS NOT NULL"));
    }
}