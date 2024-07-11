using System.Linq.Expressions;
using Raytha.Application.MediaItems;
using Raytha.Application.Themes.WebTemplates;
using Raytha.Domain.Entities;

namespace Raytha.Application.Themes;

public class ThemeJson
{
    public required string Title { get; init; }
    public required string DeveloperName { get; init; }
    public required string Description { get; init; }
    public IEnumerable<WebTemplateJson> WebTemplates { get; init; } = new List<WebTemplateJson>();
    public IEnumerable<MediaItemsJson> MediaItems { get; set; } = new List<MediaItemsJson>();

    public static Expression<Func<Theme, ThemeJson>> GetProjection()
    {
        return entity => GetProjection(entity);
    }

    public static ThemeJson GetProjection(Theme entity)
    {
        return new ThemeJson
        {
            Title = entity.Title,
            DeveloperName = entity.DeveloperName,
            Description = entity.Description,
            WebTemplates = entity.WebTemplates.Select(WebTemplateJson.GetProjection),
            MediaItems = entity.ThemeAccessToMediaItems.Select(t => MediaItemsJson.GetProjection(t.MediaItem!))
        };
    }
}