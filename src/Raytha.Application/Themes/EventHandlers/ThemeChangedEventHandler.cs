using MediatR;
using Microsoft.EntityFrameworkCore;
using Raytha.Application.Common.Interfaces;
using Raytha.Domain.Entities;
using Raytha.Domain.Events;

namespace Raytha.Application.Themes.EventHandlers;

public class ThemeChangedEventHandler : INotificationHandler<ThemeChangedEvent>
{
    private readonly IRaythaDbContext _db;

    public ThemeChangedEventHandler(IRaythaDbContext db)
    {
        _db = db;
    }

    public async Task Handle(ThemeChangedEvent notification, CancellationToken cancellationToken)
    {
        var themeWebTemplatesMappings = await _db.ThemeWebTemplatesMappings
            .Where(twt => twt.ThemeId == notification.ThemeId)
            .ToListAsync(cancellationToken);

        //set web-templates for content items 
        var contentItemDetailViewWebTemplateId = await _db.WebTemplates
            .Where(wt => wt.ThemeId == notification.ThemeId)
            .Where(wt => wt.DeveloperName == BuiltInWebTemplate.ContentItemDetailViewPage)
            .Select(wt => wt.Id)
            .FirstAsync(cancellationToken);

        var contentItems = await _db.ContentItems
            .ToListAsync(cancellationToken);

        foreach (var contentItem in contentItems)
        {
            var webTemplateId = themeWebTemplatesMappings.FirstOrDefault(twt => twt.ContentItemId == contentItem.Id)?.WebTemplateId ?? contentItemDetailViewWebTemplateId;

            contentItem.WebTemplateId = webTemplateId;
            _db.ContentItems.Update(contentItem);
        }

        //set web-templates for views
        var contentItemListViewWebTemplateId = await _db.WebTemplates
            .Where(wt => wt.ThemeId == notification.ThemeId)
            .Where(wt => wt.DeveloperName == BuiltInWebTemplate.ContentItemListViewPage)
            .Select(wt => wt.Id)
            .FirstAsync(cancellationToken);

        var views = await _db.Views
            .ToListAsync(cancellationToken);

        foreach (var view in views)
        {
            var webTemplateId = themeWebTemplatesMappings.FirstOrDefault(twt => twt.ViewId == view.Id)?.WebTemplateId ?? contentItemListViewWebTemplateId;

            view.WebTemplateId = webTemplateId;
            _db.Views.Update(view);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}