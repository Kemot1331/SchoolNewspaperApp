using Microsoft.EntityFrameworkCore;
using SchoolNewspaperBlazorApp.Data;
using SchoolNewspaperBlazorApp.Interfaces.Repository;

namespace SchoolNewspaperBlazorApp.Repository
{
    public class SubstitutionRepository : ISubstitutionRepository
    {
        private readonly IDbContextFactory<NewspaperDbContext> _factory;
        private readonly ILogger<SubstitutionRepository> _logger;

        public SubstitutionRepository(IDbContextFactory<NewspaperDbContext> factory, ILogger<SubstitutionRepository> logger)
        {
            _factory = factory;
            _logger = logger;
        }

        public async Task AddAsync(Substitution substitution)
        {
            await using var context = await _factory.CreateDbContextAsync();
            await context.Substitutions.AddAsync(substitution);
            await context.SaveChangesAsync();
        }

        public async Task<Substitution?> GetByIdAsync(int id)
        {
            await using var context = await _factory.CreateDbContextAsync();
            return await WithDetails(context.Substitutions.AsNoTracking())
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<List<Substitution>> GetListAsync(
            DateOnly? from, int? classId, int? teacherId, bool newestFirst, int take)
        {
            await using var context = await _factory.CreateDbContextAsync();

            var query = WithDetails(context.Substitutions.AsNoTracking());

            if (from is DateOnly fromDate)
            {
                query = query.Where(s => s.Date >= fromDate);
            }

            if (classId is int cid)
            {
                query = query.Where(s => s.ClassId == cid);
            }

            if (teacherId is int tid)
            {
                query = query.Where(s => s.AbsentTeacherId == tid || s.SubstituteTeacherId == tid);
            }

            query = newestFirst
                ? query.OrderByDescending(s => s.Date).ThenBy(s => s.Lesson).ThenBy(s => s.Class.Name)
                : query.OrderBy(s => s.Date).ThenBy(s => s.Lesson).ThenBy(s => s.Class.Name);

            return await query.Take(take).ToListAsync();
        }

        public async Task UpdateAsync(int id, SubstitutionInput input)
        {
            await using var context = await _factory.CreateDbContextAsync();
            var substitution = await context.Substitutions.FirstOrDefaultAsync(s => s.Id == id);

            if (substitution == null)
            {
                throw new Exception($"Substitution with ID {id} not found.");
            }

            // twórca i data utworzenia zostają bez zmian
            substitution.Date = input.Date;
            substitution.Lesson = input.Lesson;
            substitution.Kind = input.Kind;
            substitution.ClassId = input.ClassId;
            substitution.SubjectId = input.SubjectId;
            substitution.AbsentTeacherId = input.AbsentTeacherId;
            substitution.SubstituteTeacherId = input.SubstituteTeacherId;
            substitution.RoomId = input.RoomId;
            substitution.Note = input.Note ?? string.Empty;

            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            await using var context = await _factory.CreateDbContextAsync();
            var substitution = await context.Substitutions.FirstOrDefaultAsync(s => s.Id == id);

            if (substitution == null)
            {
                throw new Exception($"Substitution with ID {id} not found.");
            }

            substitution.IsDeleted = true;
            await context.SaveChangesAsync();
        }

        private static IQueryable<Substitution> WithDetails(IQueryable<Substitution> query) =>
            query
                .Include(s => s.Class)
                .Include(s => s.Subject)
                .Include(s => s.AbsentTeacher)
                .Include(s => s.SubstituteTeacher)
                .Include(s => s.Room);
    }
}
