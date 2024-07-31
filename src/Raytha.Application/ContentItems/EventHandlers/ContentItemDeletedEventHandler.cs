using MediatR;
using Microsoft.EntityFrameworkCore;
using Raytha.Application.Common.Interfaces;
using Raytha.Application.Common.Shared;
using Raytha.Domain.Events;
using Raytha.Domain.ValueObjects;

namespace Raytha.Application.ContentItems.EventHandlers;

public class ContentItemDeletedEventHandler : INotificationHandler<ContentItemDeletedEvent>
{
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly IRaythaDbContext _db;

    public ContentItemDeletedEventHandler(
        IBackgroundTaskQueue taskQueue,
        IRaythaDbContext db)
    {
        _taskQueue = taskQueue;
        _db = db;
    }

    public async Task Handle(ContentItemDeletedEvent notification, CancellationToken cancellationToken)
    {
        var activeFunctions = _db.RaythaFunctions.Where(p => p.IsActive && p.TriggerType == RaythaFunctionTriggerType.ContentItemDeleted.DeveloperName);
        if (activeFunctions.Any())
        {
            var activeThemeId = await _db.OrganizationSettings
                .Select(os => os.ActiveThemeId)
                .FirstAsync(cancellationToken);

            var webTemplate = await _db.ThemeWebTemplateContentItemMappings
                .Where(wtm => wtm.ThemeId == activeThemeId)
                .Where(wtm => wtm.ContentItemId == notification.ContentItem.Id)
                .Select(wtm => wtm.WebTemplate)
                .FirstAsync(cancellationToken);

            foreach (var activeFunction in activeFunctions)
            {
                await _taskQueue.EnqueueAsync<RaythaFunctionAsBackgroundTask>(new RaythaFunctionAsBackgroundTaskPayload 
                {
                    Target = ContentItemDto.GetProjection(notification.ContentItem, webTemplate!),
                    RaythaFunction = activeFunction
                }, cancellationToken);
            }
        }
    }
}
