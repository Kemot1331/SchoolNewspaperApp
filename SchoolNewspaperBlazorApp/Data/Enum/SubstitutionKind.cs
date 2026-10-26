namespace SchoolNewspaperBlazorApp.Data;

public enum SubstitutionKind
{
    Substitution,
    Cancelled,
    RoomChange
}

public static class SubstitutionKindExtensions
{
    public static string ToLabel(this SubstitutionKind kind) => kind switch
    {
        SubstitutionKind.Substitution => "Zastępstwo",
        SubstitutionKind.Cancelled => "Lekcja odwołana",
        SubstitutionKind.RoomChange => "Zmiana sali",
        _ => kind.ToString()
    };
}
