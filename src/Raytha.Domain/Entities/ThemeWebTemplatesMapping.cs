namespace Raytha.Domain.Entities;

public class ThemeWebTemplatesMapping : BaseEntity
{
    public required Guid WebTemplateId { get; set; }
    public virtual WebTemplate? WebTemplate { get; set; }
    public required Guid ThemeId { get; set; }
    public virtual Theme? Theme { get; set; }
    public Guid? ContentItemId { get; set; }
    public virtual ContentItem? ContentItem { get; set; }
    public Guid? ViewId { get; set; }
    public virtual View? View { get; set; }
}