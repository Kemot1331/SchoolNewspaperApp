using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolNewspaperBlazorApp.Data
{
    public class User
    {
        public int Id { get; set; }
        public required string Login { get; set; }
        public required string Password { get; set; }
        public required string Email { get; set; }
        public required DateTime RegisterDate { get; set; }

        // W bazie kolumna nazywa się AddedBy
        [Column("AddedBy")]
        public string AddBy { get; set; } = string.Empty;
    }
}