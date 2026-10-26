using SchoolNewspaperBlazorApp.Data;

namespace SchoolNewspaperBlazorApp.Interfaces.Service
{
    public interface ISubstitutionService
    {
        Task AddAsync(SubstitutionInput input);
        Task UpdateAsync(int id, SubstitutionInput input);
        Task DeleteAsync(int id);
        Task<Substitution?> GetByIdAsync(int id);
        // Publiczna lista: od dziś wzwyż, rosnąco po dacie i lekcji
        Task<List<Substitution>> GetUpcomingAsync(int? classId = null, int? teacherId = null);
        // Lista dla panelu: najnowsze na górze
        Task<List<Substitution>> GetRecentAsync();
    }
}
