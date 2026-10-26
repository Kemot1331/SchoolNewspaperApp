using SchoolNewspaperBlazorApp.Data;

namespace SchoolNewspaperBlazorApp.Interfaces.Service
{
    public interface IDictionaryService
    {
        Task<List<DictionaryRow>> GetAllAsync(DictionaryKind kind);
        Task<bool> CanManageAsync(DictionaryKind kind);
        Task AddAsync(DictionaryKind kind, string name);
        Task UpdateAsync(DictionaryKind kind, int id, string name);
        Task DeleteAsync(DictionaryKind kind, int id);
    }
}
