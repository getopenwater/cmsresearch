namespace Raytha.Domain.Entities;

public class Theme : BaseAuditableEntity, IPassivable
{
    public const string DEFAULT_THEME_DEVELOPER_NAME = "raytha_default_theme";
    public const string PREVIEW_IMAGE_FILENAME = "preview_image";

    public required string Title { get; set; }
    public required string DeveloperName { get; set; }
    public required string Description { get; set; }
    public bool IsActive { get; set; }
    public bool IsCanExport { get; set; }
    public Guid? PreviewImageId { get; set; }
    public virtual MediaItem? PreviewImage { get; set; }
    public virtual ICollection<ThemeAccessToMediaItem> ThemeAccessToMediaItems{ get; set; } = new List<ThemeAccessToMediaItem>();
    public virtual ICollection<WebTemplate> WebTemplates { get; set; } = new List<WebTemplate>();
    public virtual ICollection<ThemeRevision> Revisions { get; set; } = new List<ThemeRevision>();
    public virtual ICollection<ThemeWebTemplatesMapping> WebTemplatesMappings { get; set; } = new List<ThemeWebTemplatesMapping>();
}