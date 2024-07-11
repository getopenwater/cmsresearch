namespace Raytha.Domain.Events;

public class ThemeCreatedEvent : BaseEvent, IAfterSaveChangesNotification
{
    public Theme Theme { get; set; }
    public string? ImageBase64 { get; set; }
    public string? ImageFileType { get; set; }
    public string? ImageFileName { get; set; }
    public bool InsertDefaultMediaItems { get; set; }

    public ThemeCreatedEvent(Theme theme, string? imageBase64, string? imageFileName, string? imageFileType, bool insertDefaultMediaItems)
    {
        Theme = theme;
        ImageBase64 = imageBase64;
        ImageFileName = imageFileName;
        ImageFileType = imageFileType;
        InsertDefaultMediaItems = insertDefaultMediaItems;
    }
}