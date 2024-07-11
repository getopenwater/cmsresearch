using System.Linq.Expressions;
using Raytha.Domain.Entities;

namespace Raytha.Application.Themes.WebTemplates;

public class WebTemplateJson
{
    public required Guid Id { get; init; }
    public Guid ThemeId { get; init; }
    public bool IsBaseLayout { get; init; }
    public required string Label { get; init; }
    public required string DeveloperName { get; init; }
    public required string Content { get; init; }
    public bool IsBuiltInTemplate { get; init; }
    public Guid? ParentTemplateId { get; init; }
    public bool AllowAccessForNewContentTypes { get; init; }
    public required IEnumerable<WebTemplateAccessToModelDefinitionJson> TemplateAccessToModelDefinitions { get; init; }

    public static Expression<Func<WebTemplate, WebTemplateJson>> GetProjection()
    {
        return entity => GetProjection(entity);
    }

    public static WebTemplateJson GetProjection(WebTemplate entity)
    {
        return new WebTemplateJson
        {
            Id = entity.Id,
            ThemeId = entity.ThemeId,
            IsBaseLayout = entity.IsBaseLayout,
            AllowAccessForNewContentTypes = entity.AllowAccessForNewContentTypes,
            Label = entity.Label!,
            Content = entity.Content!,
            DeveloperName = entity.DeveloperName!,
            IsBuiltInTemplate = entity.IsBuiltInTemplate,
            ParentTemplateId = entity.ParentTemplateId,
            TemplateAccessToModelDefinitions = entity.TemplateAccessToModelDefinitions.Select(t => new WebTemplateAccessToModelDefinitionJson
            {
                Id = t.Id,
                ContentTypeId = t.ContentTypeId,
                WebTemplateId = t.WebTemplateId,
            }),
        };
    }

    public class WebTemplateAccessToModelDefinitionJson
    {
        public Guid Id { get; set; }
        public Guid WebTemplateId { get; set; }
        public Guid ContentTypeId { get; set; }
    }
}