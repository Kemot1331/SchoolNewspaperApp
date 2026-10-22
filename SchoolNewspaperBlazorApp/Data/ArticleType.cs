namespace SchoolNewspaperBlazorApp.Data;

public enum ArticleType
{
    Announcements,
    Substitute,
    Events,
    Competitions,
    Admisions,
    Exams,
    StudentCouncil
}

public static class EventsTypesExtensions
{
    public static string ToLabel(this ArticleType type) => type switch
    {
        ArticleType.Announcements => "Ogłoszenia",
        ArticleType.Substitute => "Zastępstwa",
        ArticleType.Events => "Wydarzenia",
        ArticleType.Competitions => "Zawody",
        ArticleType.Admisions => "Rekrutacja",
        ArticleType.Exams => "Egzaminy",
        ArticleType.StudentCouncil => "Samorząd uczniowski",
        _ => type.ToString()
    };
}