using SchoolNewspaperBlazorApp.Data;

namespace SchoolNewspaperBlazorApp.Interfaces.Repository
{
    public interface IDictionaryRepository
    {
        Task<List<DictionaryRow>> GetAllAsync(DictionaryKind kind);
        Task<int> AddAsync(DictionaryKind kind, string name);
        Task UpdateAsync(DictionaryKind kind, int id, string name);
        Task DeleteAsync(DictionaryKind kind, int id);
        Task<bool> IsNameTakenAsync(DictionaryKind kind, int excludeId, string name);
        // true, jeśli wpis jest użyty w jakimkolwiek zastępstwie (także usuniętym)
        Task<bool> IsInUseAsync(DictionaryKind kind, int id);
    }
}
