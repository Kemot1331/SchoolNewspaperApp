using Microsoft.EntityFrameworkCore;
using SchoolNewspaperBlazorApp.Data;
using SchoolNewspaperBlazorApp.Interfaces.Repository;

namespace SchoolNewspaperBlazorApp.Repository
{
    // Cztery słowniki mają ten sam kształt (Id + Name), więc jedna implementacja obsługuje wszystkie.
    public class DictionaryRepository : IDictionaryRepository
    {
        private readonly IDbContextFactory<NewspaperDbContext> _factory;
        private readonly ILogger<DictionaryRepository> _logger;

        public DictionaryRepository(IDbContextFactory<NewspaperDbContext> factory, ILogger<DictionaryRepository> logger)
        {
            _factory = factory;
            _logger = logger;
        }

        public async Task<List<DictionaryRow>> GetAllAsync(DictionaryKind kind)
        {
            await using var context = await _factory.CreateDbContextAsync();
            return await GetOps(kind, context).ListAsync();
        }

        public async Task<int> AddAsync(DictionaryKind kind, string name)
        {
            await using var context = await _factory.CreateDbContextAsync();
            return await GetOps(kind, context).AddAsync(name);
        }

        public async Task UpdateAsync(DictionaryKind kind, int id, string name)
        {
            await using var context = await _factory.CreateDbContextAsync();
            await GetOps(kind, context).UpdateAsync(id, name);
        }

        public async Task DeleteAsync(DictionaryKind kind, int id)
        {
            await using var context = await _factory.CreateDbContextAsync();
            await GetOps(kind, context).DeleteAsync(id);
        }

        public async Task<bool> IsNameTakenAsync(DictionaryKind kind, int excludeId, string name)
        {
            await using var context = await _factory.CreateDbContextAsync();
            return await GetOps(kind, context).NameTakenAsync(excludeId, name);
        }

        public async Task<bool> IsInUseAsync(DictionaryKind kind, int id)
        {
            await using var context = await _factory.CreateDbContextAsync();

            // IgnoreQueryFilters: usunięte (soft delete) zastępstwa nadal trzymają klucz obcy
            var substitutions = context.Substitutions.IgnoreQueryFilters();

            return kind switch
            {
                DictionaryKind.Class => await substitutions.AnyAsync(s => s.ClassId == id),
                DictionaryKind.Teacher => await substitutions.AnyAsync(s =>
                    s.AbsentTeacherId == id || s.SubstituteTeacherId == id),
                DictionaryKind.Room => await substitutions.AnyAsync(s => s.RoomId == id),
                DictionaryKind.Subject => await substitutions.AnyAsync(s => s.SubjectId == id),
                _ => false
            };
        }

        private static IOps GetOps(DictionaryKind kind, NewspaperDbContext context) => kind switch
        {
            DictionaryKind.Class => new Ops<SchoolClass>(context),
            DictionaryKind.Teacher => new Ops<Teacher>(context),
            DictionaryKind.Room => new Ops<Room>(context),
            DictionaryKind.Subject => new Ops<Subject>(context),
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };

        private interface IOps
        {
            Task<List<DictionaryRow>> ListAsync();
            Task<int> AddAsync(string name);
            Task UpdateAsync(int id, string name);
            Task DeleteAsync(int id);
            Task<bool> NameTakenAsync(int excludeId, string name);
        }

        private sealed class Ops<T> : IOps where T : DictionaryItem, new()
        {
            private readonly NewspaperDbContext _context;

            public Ops(NewspaperDbContext context) => _context = context;

            public Task<List<DictionaryRow>> ListAsync() =>
                _context.Set<T>()
                    .AsNoTracking()
                    .OrderBy(x => x.Name)
                    .Select(x => new DictionaryRow(x.Id, x.Name))
                    .ToListAsync();

            public async Task<int> AddAsync(string name)
            {
                var item = new T { Name = name };
                await _context.Set<T>().AddAsync(item);
                await _context.SaveChangesAsync();
                return item.Id;
            }

            public async Task UpdateAsync(int id, string name)
            {
                var item = await _context.Set<T>().FirstOrDefaultAsync(x => x.Id == id)
                           ?? throw new InvalidOperationException("Nie znaleziono wpisu.");
                item.Name = name;
                await _context.SaveChangesAsync();
            }

            public async Task DeleteAsync(int id)
            {
                var item = await _context.Set<T>().FirstOrDefaultAsync(x => x.Id == id)
                           ?? throw new InvalidOperationException("Nie znaleziono wpisu.");
                _context.Set<T>().Remove(item);
                await _context.SaveChangesAsync();
            }

            public Task<bool> NameTakenAsync(int excludeId, string name) =>
                _context.Set<T>().AnyAsync(x => x.Id != excludeId && x.Name == name);
        }
    }
}
