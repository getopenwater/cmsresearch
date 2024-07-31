using Raytha.Application.Common.Models;
using System.Linq.Expressions;
using CSharpVitamins;

namespace Raytha.Application.Themes;

public record ThemeWebTemplateMappingDto : BaseEntityDto
{
    public required bool AnyWebTemplateContentItemMappings { get; init; }
    public required bool AnyWebTemplateViewMappings { get; init; }
    public bool IsWebTemplateMatchingRequired => !AnyWebTemplateContentItemMappings || !AnyWebTemplateViewMappings;

    public static Expression<Func<bool, bool, ThemeWebTemplateMappingDto>> GetProjection()
    {
        return (anyWebTemplateContentItemMappings, anyWebTemplateViewMappings) => GetProjection(anyWebTemplateContentItemMappings, anyWebTemplateViewMappings);
    }

    public static ThemeWebTemplateMappingDto GetProjection(bool anyWebTemplateContentItemMappings, bool anyWebTemplateViewMappings)
    {
        return new ThemeWebTemplateMappingDto
        {
            Id = ShortGuid.NewGuid(),
            AnyWebTemplateContentItemMappings = anyWebTemplateContentItemMappings,
            AnyWebTemplateViewMappings = anyWebTemplateViewMappings,
        };
    }
}