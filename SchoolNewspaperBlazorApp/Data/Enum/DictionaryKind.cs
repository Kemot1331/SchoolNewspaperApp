namespace SchoolNewspaperBlazorApp.Data;

// Cztery słowniki używane w formularzu zastępstw
public enum DictionaryKind
{
    Class,
    Teacher,
    Room,
    Subject
}

public static class DictionaryKindExtensions
{
    // Nazwa w liczbie mnogiej – zakładki i nagłówki
    public static string ToLabel(this DictionaryKind kind) => kind switch
    {
        DictionaryKind.Class => "Klasy",
        DictionaryKind.Teacher => "Nauczyciele",
        DictionaryKind.Room => "Sale",
        DictionaryKind.Subject => "Przedmioty",
        _ => kind.ToString()
    };

    // Nazwa w liczbie pojedynczej – komunikaty i etykiety pól
    public static string ToSingular(this DictionaryKind kind) => kind switch
    {
        DictionaryKind.Class => "Klasa",
        DictionaryKind.Teacher => "Nauczyciel",
        DictionaryKind.Room => "Sala",
        DictionaryKind.Subject => "Przedmiot",
        _ => kind.ToString()
    };

    public static string ToPlaceholder(this DictionaryKind kind) => kind switch
    {
        DictionaryKind.Class => "Np. 3A",
        DictionaryKind.Teacher => "Np. Jan Kowalski",
        DictionaryKind.Room => "Np. 214",
        DictionaryKind.Subject => "Np. Matematyka",
        _ => ""
    };

    public static string ToIcon(this DictionaryKind kind) => kind switch
    {
        DictionaryKind.Class => "🎓",
        DictionaryKind.Teacher => "👤",
        DictionaryKind.Room => "🚪",
        DictionaryKind.Subject => "📘",
        _ => "•"
    };

    // Maksymalna długość nazwy (zgodna z konfiguracją w NewspaperDbContext)
    public static int MaxNameLength(this DictionaryKind kind) => kind switch
    {
        DictionaryKind.Class => 20,
        DictionaryKind.Teacher => 100,
        DictionaryKind.Room => 30,
        DictionaryKind.Subject => 100,
        _ => 100
    };

    // Każdy słownik ma własne uprawnienie
    public static EnumPermissions RequiredPermission(this DictionaryKind kind) => kind switch
    {
        DictionaryKind.Class => EnumPermissions.ManageClasses,
        DictionaryKind.Teacher => EnumPermissions.ManageTeachers,
        DictionaryKind.Room => EnumPermissions.ManageRooms,
        DictionaryKind.Subject => EnumPermissions.ManageSubjects,
        _ => EnumPermissions.Master
    };
}
