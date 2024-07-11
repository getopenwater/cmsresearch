namespace Raytha.Domain.Events;

public class ThemeChangedEvent : BaseEvent, IBeforeSaveChangesNotification
{
    public Guid ThemeId { get; set; }

    public ThemeChangedEvent(Guid themeId)
    {
        ThemeId = themeId;
    }
}