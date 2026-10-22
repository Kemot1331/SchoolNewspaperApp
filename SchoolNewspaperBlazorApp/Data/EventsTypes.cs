namespace SchoolNewspaperBlazorApp.Data;

public enum EventsTypes
{
    trips,
    academies,
    events
}

public static class EventsTypesExtensions
{
    public static string ToLabel(this EventsTypes type) => type switch
    {
        EventsTypes.trips => "Wycieczki",
        EventsTypes.academies => "Akademie",
        EventsTypes.events => "Imprezy szkolne",
        _ => type.ToString()
    };
}