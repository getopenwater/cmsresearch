namespace Raytha.Domain.Entities;

public class ThemeRevision : BaseAuditableEntity
{
    public required Guid ThemeId { get; set; }
    public virtual Theme? Theme { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string WebTemplatesJson { get; set; }
    public required string WebTemplatesMappingJson { get; set; }
}