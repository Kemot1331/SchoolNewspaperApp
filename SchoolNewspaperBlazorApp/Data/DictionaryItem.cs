namespace SchoolNewspaperBlazorApp.Data;

// Wspólny kształt czterech słowników. Klasa bazowa NIE jest encją EF –
// każdy słownik ma własną tabelę (SchoolClasses, Teachers, Rooms, Subjects).
public abstract class DictionaryItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class SchoolClass : DictionaryItem { }

public class Teacher : DictionaryItem { }

public class Room : DictionaryItem { }

public class Subject : DictionaryItem { }

// Lekka reprezentacja wiersza słownika (do list i <select>)
public record DictionaryRow(int Id, string Name);
