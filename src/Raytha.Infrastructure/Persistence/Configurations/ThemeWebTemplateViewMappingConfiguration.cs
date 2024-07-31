using Microsoft.EntityFrameworkCore;
using Raytha.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Raytha.Infrastructure.Persistence.Configurations;

public class ThemeWebTemplateViewMappingConfiguration : IEntityTypeConfiguration<ThemeWebTemplateViewMapping>
{
    public void Configure(EntityTypeBuilder<ThemeWebTemplateViewMapping> builder)
    {
        builder
            .HasOne(wtm => wtm.WebTemplate)
            .WithMany()
            .HasForeignKey(t => t.WebTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(wtm => wtm.View)
            .WithMany()
            .HasForeignKey(wtm => wtm.ViewId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(wtm => new { wtm.ThemeId, wtm.ViewId, wtm.WebTemplateId })
            .IsUnique();
    }
}