using Microsoft.EntityFrameworkCore;
using SchoolNewspaperBlazorApp.Data;
using SchoolNewspaperBlazorApp.Interfaces.Repository;

namespace SchoolNewspaperBlazorApp.Repository
{
    public class FileRepository : IFileRepository
    {
        private readonly IDbContextFactory<NewspaperDbContext> _factory;

        public FileRepository(IDbContextFactory<NewspaperDbContext> factory)
        {
            _factory = factory;
        }

        public async Task<int> GetLastFileId()
        {
            await using var context = await _factory.CreateDbContextAsync();
            var id = await context.Files
                .OrderByDescending(x => x.Id)
                .Select(x => x.Id)
                .FirstOrDefaultAsync();
            return id + 1;
        }

        public async Task<MediaFile> GetFileByIdAsync(int id)
        {
            await using var context = await _factory.CreateDbContextAsync();
            var file = await context.Files
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);

            if (file == null)
            {
                throw new Exception("[FileRepository]Brak zdjecia w bazie danych");
            }
            return file;
        }

        public async Task AddFileAsync(MediaFile file)
        {
            await using var context = await _factory.CreateDbContextAsync();
            context.Files.Add(file);
            await context.SaveChangesAsync();
        }
    }
}
