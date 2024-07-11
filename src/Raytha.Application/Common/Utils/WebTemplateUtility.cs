using Microsoft.EntityFrameworkCore;
using Raytha.Application.Common.Interfaces;
using Raytha.Domain.Entities;

namespace Raytha.Application.Common.Utils;

public class WebTemplateUtility
{
    public static async Task LoadParentWebTemplatesRecursiveAsync(WebTemplate webTemplate, IRaythaDbContext db, CancellationToken cancellationToken)
    {
        var parentWebTemplate = await db.WebTemplates.FirstAsync(p => p.Id == webTemplate.ParentTemplateId, cancellationToken);

        webTemplate.ParentTemplate = parentWebTemplate;

        if (parentWebTemplate.ParentTemplateId != null)
            await LoadParentWebTemplatesRecursiveAsync(parentWebTemplate, db, cancellationToken);
    }
}