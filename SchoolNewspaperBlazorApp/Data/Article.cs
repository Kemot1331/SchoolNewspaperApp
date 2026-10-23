using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolNewspaperBlazorApp.Data
{
    public class Article
    {
        public int Id { get; set; }
        public required string Title { get; set; }

        public required string Text { get; set; }

        // Zrzut imienia i nazwiska twórcy z chwili dodania (artykuły sprzed zmian mają tu wpisanego autora ręcznie)
        public string Author { get; set; } = string.Empty;
        public int? UserId { get; set; }
        public User? User { get; set; }
        public DateTime PublishDate { get; set; }
        public int FileId { get; set; }
        public ICollection<ArticleImage> Images { get; set; }
        public ArticleType? EventsType { get; set; }
        public bool isDeleted { get; set; } = false;

        // "Imię Nazwisko · Rola" – na podstawie danych użytkownika; dla starych artykułów samo Author
        [NotMapped]
        public string AuthorDisplay
        {
            get
            {
                var name = User is null ? Author : $"{User.FirstName} {User.LastName}".Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    name = string.IsNullOrWhiteSpace(Author) ? User?.Login ?? "" : Author;
                }

                var role = User?.Role?.RoleName;
                return string.IsNullOrWhiteSpace(role) ? name : $"{name} · {role}";
            }
        }
    }
}