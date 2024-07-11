using System.Linq.Expressions;
using Raytha.Domain.Entities;

namespace Raytha.Application.Themes;

public class ThemeWebTemplatesMappingJson
{
    public Guid Id { get; set; }
    public Guid ThemeId { get; set; }
    public Guid WebTemplateId { get; set; }
    public Guid? ContentItemId { get; set; }
    public Guid? ViewId { get; set; }

    public static Expression<Func<ThemeWebTemplatesMapping, ThemeWebTemplatesMappingJson>> GetProjection()
    {
        return entity => GetProjection(entity);
    }

    public static ThemeWebTemplatesMappingJson GetProjection(ThemeWebTemplatesMapping entity)
    {
        return new ThemeWebTemplatesMappingJson
        {
            Id = entity.Id,
            ThemeId = entity.ThemeId,
            WebTemplateId = entity.WebTemplateId,
            ContentItemId = entity.ContentItemId,
            ViewId = entity.ViewId,
        };
    }
}