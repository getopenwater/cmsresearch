using System.Linq.Expressions;
using Raytha.Domain.Entities;

namespace Raytha.Application.MediaItems;

public class MediaItemsJson
{
    public required string FileName { get; init; }
    public bool IsPreviewImage { get; set; }
    public string DownloadUrl { get; set; }

    public static Expression<Func<MediaItem, MediaItemsJson>> GetProjection()
    {
        return entity => GetProjection(entity);
    }

    public static MediaItemsJson GetProjection(MediaItem entity)
    {
        return new MediaItemsJson
        {
            FileName = entity.FileName,
        };
    }
}