namespace SchoolNewspaperBlazorApp.Data;

public enum ArticleType
{
    news,
    events,
    announcement 
}

public static class EventsTypesExtensions
{
    public static string ToLabel(this ArticleType type) => type switch
    {
        ArticleType.news => "Nowości",
        ArticleType.events => "Wydarzenia",
        ArticleType.announcement => "Ogłoszenia",
        _ => type.ToString()
    };
}