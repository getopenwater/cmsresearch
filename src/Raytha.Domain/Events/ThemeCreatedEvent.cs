namespace Raytha.Domain.Events;

public class ThemeCreatedEvent : BaseEvent, IAfterSaveChangesNotification
{
    public Theme Theme { get; }
    public string? ImageBase64 { get; }
    public string? ImageFileType { get; }
    public string? ImageFileName { get; }
    public bool InsertDefaultMediaItems { get; }

    public ThemeCreatedEvent(Theme theme, string? imageBase64, string? imageFileName, string? imageFileType, bool insertDefaultMediaItems)
    {
        Theme = theme;
        ImageBase64 = imageBase64;
        ImageFileName = imageFileName;
        ImageFileType = imageFileType;
        InsertDefaultMediaItems = insertDefaultMediaItems;
    }
}