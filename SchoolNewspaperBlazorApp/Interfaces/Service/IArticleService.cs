
using SchoolNewspaperBlazorApp.Data;

namespace SchoolNewspaperBlazorApp.Interfaces.Service
{
    public interface IArticleService
    {
        Task AddArticleAsync(string title, string text, string author, List<int> fileIds, EventsTypes? eventType = null);
        Task<List<Article>> GetAllArticlesAsync();
        Task<Article> GetArticleByIdAsync(int id);
        Task RemoveArticleByIdAsync(int id);
        Task UpdateArticleByIdAsync(int id, string title, string text, string author, int fileId, EventsTypes? eventType = null);    }
}
