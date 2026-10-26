using SchoolNewspaperBlazorApp.Data;

namespace SchoolNewspaperBlazorApp.Interfaces.Repository
{
    public interface ISubstitutionRepository
    {
        Task AddAsync(Substitution substitution);
        Task<Substitution?> GetByIdAsync(int id);
        // from = tylko od tej daty wzwyż; classId / teacherId = filtry (nauczyciel: nieobecny LUB zastępujący)
        Task<List<Substitution>> GetListAsync(DateOnly? from, int? classId, int? teacherId, bool newestFirst, int take);
        Task UpdateAsync(int id, SubstitutionInput input);
        Task DeleteAsync(int id);
    }
}
