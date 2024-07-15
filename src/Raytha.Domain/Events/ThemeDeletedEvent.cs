namespace Raytha.Domain.Events;

public class ThemeDeletedEvent : BaseEvent, IBeforeSaveChangesNotification
{
    public Guid ThemeId { get; }

    public ThemeDeletedEvent(Guid themeId)
    {
        ThemeId = themeId;
    }
}