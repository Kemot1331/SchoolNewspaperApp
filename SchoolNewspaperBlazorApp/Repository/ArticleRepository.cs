using Microsoft.EntityFrameworkCore;
using SchoolNewspaperBlazorApp.Data;
using SchoolNewspaperBlazorApp.Interfaces.Repository;

namespace SchoolNewspaperBlazorApp.Repository
{
    public class ArticleRepository : IArticleRepository
    {
        private readonly IDbContextFactory<NewspaperDbContext> _factory;

        public ArticleRepository(IDbContextFactory<NewspaperDbContext> factory)
        {
            _factory = factory;
        }

        public async Task AddArticleAsync(Article article)
        {
            await using var context = await _factory.CreateDbContextAsync();
            await context.Articles.AddAsync(article);
            await context.SaveChangesAsync();
        }

        public async Task<List<Article>> GetAllArticlesAsync()
        {
            await using var context = await _factory.CreateDbContextAsync();
            return await context.Articles.AsNoTracking().ToListAsync();
        }

        public async Task<Article> GetArticleByIdAsync(int id)
        {
            await using var context = await _factory.CreateDbContextAsync();
            var article = await context.Articles
                .Include(a => a.Images)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (article == null)
            {
                throw new Exception($"Article with ID {id} not found.");
            }
            return article;
        }

        public async Task RemoveArticleByIdAsync(int id)
        {   
            await using var context = await _factory.CreateDbContextAsync();
            var article = await context.Articles.FirstOrDefaultAsync(a => a.Id == id);

            if (article == null)
            {
                throw new Exception($"Article with ID {id} not found.");
            }

            context.Articles.Remove(article);
            await context.SaveChangesAsync();
        }

        public async Task UpdateArticleByIdAsync(int id, Article newArticle)
        {
            await using var context = await _factory.CreateDbContextAsync();
            var articleById = await context.Articles.FirstOrDefaultAsync(a => a.Id == id);

            if (articleById == null)
            {
                throw new Exception($"Article with ID {id} not found.");
            }

            if (newArticle.FileId != 0)
            {
                articleById.FileId = newArticle.FileId;
            }
            
            articleById.Title = newArticle.Title;
            articleById.Author = newArticle.Author;
            articleById.Text = newArticle.Text;
                
             await context.SaveChangesAsync();
        }
    }
}