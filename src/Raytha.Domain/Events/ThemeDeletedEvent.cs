namespace Raytha.Domain.Events;

public class ThemeDeletedEvent : BaseEvent, IBeforeSaveChangesNotification
{
    public Guid ThemeId { get; set; }

    public ThemeDeletedEvent(Guid themeId)
    {
        ThemeId = themeId;
    }
}