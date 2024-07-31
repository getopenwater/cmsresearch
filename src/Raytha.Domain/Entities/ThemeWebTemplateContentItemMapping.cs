namespace Raytha.Domain.Entities;

public class ThemeWebTemplateContentItemMapping : BaseEntity
{
    public required Guid WebTemplateId { get; set; }
    public virtual WebTemplate? WebTemplate { get; set; }
    public required Guid ThemeId { get; set; }
    public Guid? ContentItemId { get; set; }
    public virtual ContentItem? ContentItem { get; set; }
}