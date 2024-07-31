using Raytha.Application.Themes.WebTemplates;

namespace Raytha.Application.Common.Utils;

public class WebTemplateDtoDeveloperNameComparer : IEqualityComparer<WebTemplateDto>
{
    public bool Equals(WebTemplateDto? x, WebTemplateDto? y)
    {
        if (x == null || y == null) 
            return false;

        return x.DeveloperName == y.DeveloperName;
    }

    public int GetHashCode(WebTemplateDto obj)
    {
        return obj.DeveloperName.GetHashCode();
    }
}