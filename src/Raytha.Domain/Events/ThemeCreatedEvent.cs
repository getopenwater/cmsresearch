namespace Raytha.Domain.Events;

public class ThemeCreatedEvent : BaseEvent, IAfterSaveChangesNotification
{
    public Guid ThemeId { get; }
    public bool InsertDefaultMediaItems { get; }

    public ThemeCreatedEvent(Guid themeId, bool insertDefaultMediaItems)
    {
        ThemeId = themeId;
        InsertDefaultMediaItems = insertDefaultMediaItems;
    }
}