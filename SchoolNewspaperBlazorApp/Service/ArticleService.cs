using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SchoolNewspaperBlazorApp.Data;
using SchoolNewspaperBlazorApp.Interfaces.Repository;
using SchoolNewspaperBlazorApp.Interfaces.Service;

namespace SchoolNewspaperBlazorApp.Service
{
    public class ArticleService : IArticleService
    {
        private readonly IArticleRepository _articleRepository;
        private readonly IPermissionService _permissionService;
        public ArticleService(IArticleRepository articleRepository, IPermissionService permissionService)
        {
            _articleRepository = articleRepository;
            _permissionService = permissionService;
        }
        public async Task AddArticleAsync(string title, string text, string author, List<int> fileIds)
        {
            bool hasPermission =  await _permissionService.HasPermissionAsync(EnumPermissions.AddArticle);
            if(!hasPermission) return;
            var article = new Article
            {
                Title = title,
                Text = text,
                Author = author,
                PublishDate = DateTime.Now,
                FileId = fileIds.FirstOrDefault(),
                Images = fileIds
                    .Select((id, index) => new ArticleImage { FileId = id, Order = index })
                    .ToList()   
            };
            await _articleRepository.AddArticleAsync(article);
        }
        public async Task<List<Article>> GetAllArticlesAsync()
        {
            return await _articleRepository.GetAllArticlesAsync();
        }
        public async Task<Article> GetArticleByIdAsync(int id)
        {
            Article article = await _articleRepository.GetArticleByIdAsync(id);
            if (article == null)
            {
                throw new Exception($"Article with ID {id} not found.");
            }
            return article;
        }
        public async Task RemoveArticleByIdAsync(int id)
        {
            if(id > 0)
            {
                 await _articleRepository.RemoveArticleByIdAsync(id);
            }
        }

        public async Task UpdateArticleByIdAsync(int id, string title, string text, string author, int fileId)
        {
            var article = new Article
            {
                Id = id,
                Title = title,
                Text = text,
                Author = author,
                FileId =  fileId
            };
            await _articleRepository.UpdateArticleByIdAsync(id,article);
        }
    }
}
