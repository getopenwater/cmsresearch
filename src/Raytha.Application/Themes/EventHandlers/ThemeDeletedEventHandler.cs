using MediatR;
using Microsoft.EntityFrameworkCore;
using Raytha.Application.Common.Interfaces;
using Raytha.Domain.Events;

namespace Raytha.Application.Themes.EventHandlers;

public class ThemeDeletedEventHandler : INotificationHandler<ThemeDeletedEvent>
{
    private readonly IRaythaDbContext _db;
    private readonly IFileStorageProvider _fileStorageProvider;

    public ThemeDeletedEventHandler(IRaythaDbContext db, IFileStorageProvider fileStorageProvider)
    {
        _db = db;
        _fileStorageProvider = fileStorageProvider;
    }

    public async Task Handle(ThemeDeletedEvent notification, CancellationToken cancellationToken)
    {
        var mediaItems = await _db.ThemeAccessToMediaItems
            .Where(tmi => tmi.ThemeId == notification.ThemeId)
            .Include(tmi => tmi.MediaItem)
            .Select(tmi => tmi.MediaItem)
            .ToListAsync(cancellationToken);

        foreach (var mediaItem in mediaItems)
        {
            _db.MediaItems.Remove(mediaItem!);

            await _fileStorageProvider.DeleteAsync(mediaItem!.ObjectKey);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}