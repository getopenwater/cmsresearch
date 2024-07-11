using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raytha.Domain.Entities;

namespace Raytha.Infrastructure.Persistence.Configurations;

public class ThemeRevisionConfiguration : IEntityTypeConfiguration<ThemeRevision>
{
    public void Configure(EntityTypeBuilder<ThemeRevision> builder)
    {
        builder
            .HasOne(rtr => rtr.CreatorUser)
            .WithMany()
            .HasForeignKey(rtr => rtr.CreatorUserId);

        builder
            .HasOne(rtr => rtr.LastModifierUser)
            .WithMany()
            .HasForeignKey(rtr => rtr.LastModifierUserId);
    }
}