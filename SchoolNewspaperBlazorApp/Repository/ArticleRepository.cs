using Microsoft.EntityFrameworkCore;
using SchoolNewspaperBlazorApp.Data;
using SchoolNewspaperBlazorApp.Interfaces.Repository;

namespace SchoolNewspaperBlazorApp.Repository
{
    public class ArticleRepository : IArticleRepository
    {
        private readonly IDbContextFactory<NewspaperDbContext> _factory;
        private readonly ILogger<ArticleRepository> _logger;

        public ArticleRepository(IDbContextFactory<NewspaperDbContext> factory, ILogger<ArticleRepository> logger)
        {
            _factory = factory;
            _logger = logger;
        }

        public async Task AddArticleAsync(Article article)
        {
            await using var context = await _factory.CreateDbContextAsync();
            await context.Articles.AddAsync(article);
            await context.SaveChangesAsync();
        }

        // Najnowsze na górze – strona główna i panel dostają tę samą kolejność (wcześniej była to kolejność z bazy)
        public async Task<List<Article>> GetAllArticlesAsync()
        {
            await using var context = await _factory.CreateDbContextAsync();
            return await context.Articles
                .AsNoTracking()
                .Include(a => a.User)
                    .ThenInclude(u => u!.Role)
                .OrderByDescending(a => a.PublishDate)
                .ThenByDescending(a => a.Id)
                .ToListAsync();
        }

        public async Task<Article> GetArticleByIdAsync(int id)
        {
            await using var context = await _factory.CreateDbContextAsync();
            var article = await context.Articles
                .Include(a => a.Images)
                .Include(a => a.User)
                    .ThenInclude(u => u!.Role)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (article == null)
            {
                throw new KeyNotFoundException($"Article with ID {id} not found.");
            }
            return article;
        }

        public async Task RemoveArticleByIdAsync(int id)
        {
            await using var context = await _factory.CreateDbContextAsync();
            var article = await context.Articles.FirstOrDefaultAsync(a => a.Id == id);

            if (article == null)
            {
                throw new KeyNotFoundException($"Article with ID {id} not found.");
            }

            article.IsDeleted = true;
            await context.SaveChangesAsync();
        }

        public async Task UpdateArticleByIdAsync(int id, Article newArticle)
        {
            await using var context = await _factory.CreateDbContextAsync();
            var articleById = await context.Articles
                .Include(a => a.Images)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (articleById == null)
            {
                throw new KeyNotFoundException($"Article with ID {id} not found.");
            }

            // Nowa okładka ZASTĘPUJE starą: podmieniamy też wpis w galerii. Wcześniej stara okładka zostawała
            // w Images i po edycji pojawiała się jako dodatkowe zdjęcie pod artykułem.
            if (newArticle.FileId != 0 && newArticle.FileId != articleById.FileId)
            {
                articleById.Images ??= new List<ArticleImage>();

                var oldCover = articleById.Images.FirstOrDefault(i => i.FileId == articleById.FileId);
                if (oldCover != null)
                {
                    oldCover.FileId = newArticle.FileId;
                }
                else
                {
                    articleById.Images.Add(new ArticleImage { FileId = newArticle.FileId, Order = 0 });
                }

                articleById.FileId = newArticle.FileId;
            }

            articleById.Title = newArticle.Title;
            articleById.Text = newArticle.Text;
            articleById.EventsType = newArticle.EventsType;

            await context.SaveChangesAsync();
        }
    }
}
