using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolNewspaperBlazorApp.Data
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public required string Login { get; set; }
        public required string Password { get; set; }
        public required string Email { get; set; }
        public required DateTime RegisterDate { get; set; }

        // W bazie kolumna nazywa się AddedBy
        [Column("AddedBy")]
        public string AddBy { get; set; } = string.Empty;

        // Soft delete: 0 = aktywne konto, 1 = usunięte (wiersz zostaje w bazie)
        public bool IsDeleted { get; set; }
        public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
        public Role? Role { get; set; }
    }
}