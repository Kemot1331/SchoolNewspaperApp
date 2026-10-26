using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolNewspaperBlazorApp.Data;

public class Substitution
{
    public const int MaxNoteLength = 500;
    // Lekcja 0 = zerowa
    public const int MaxLesson = 12;

    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public int Lesson { get; set; }
    public SubstitutionKind Kind { get; set; }

    public int ClassId { get; set; }
    public SchoolClass Class { get; set; } = null!;

    public int? SubjectId { get; set; }
    public Subject? Subject { get; set; }

    // Nauczyciel, który miał prowadzić lekcję (nieobecny albo ten, któremu zmieniono salę)
    public int? AbsentTeacherId { get; set; }
    public Teacher? AbsentTeacher { get; set; }

    // Nauczyciel zastępujący (tylko przy zastępstwie)
    public int? SubstituteTeacherId { get; set; }
    public Teacher? SubstituteTeacher { get; set; }

    public int? RoomId { get; set; }
    public Room? Room { get; set; }

    public string Note { get; set; } = string.Empty;

    public int? CreatedByUserId { get; set; }
    public User? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    // Soft delete jak przy artykułach i użytkownikach
    public bool IsDeleted { get; set; }

    [NotMapped]
    public string LessonLabel => Lesson == 0 ? "0" : Lesson.ToString();
}

// Dane z formularza zastępstwa (bez encji EF – walidacja jest w SubstitutionService)
public record SubstitutionInput(
    DateOnly Date,
    int Lesson,
    SubstitutionKind Kind,
    int ClassId,
    int? SubjectId,
    int? AbsentTeacherId,
    int? SubstituteTeacherId,
    int? RoomId,
    string? Note);
