using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using SchoolNewspaperBlazorApp.Data;
using SchoolNewspaperBlazorApp.Interfaces.Repository;
using SchoolNewspaperBlazorApp.Interfaces.Service;

namespace SchoolNewspaperBlazorApp.Service
{
    public class ArticleService : IArticleService
    {
        private readonly IArticleRepository _articleRepository;
        private readonly IAuthRepository _authRepository;
        private readonly IPermissionService _permissionService;
        private readonly AuthenticationStateProvider _authenticationStateProvider;

        public ArticleService(
            IArticleRepository articleRepository,
            IAuthRepository authRepository,
            IPermissionService permissionService,
            AuthenticationStateProvider authenticationStateProvider)
        {
            _articleRepository = articleRepository;
            _authRepository = authRepository;
            _permissionService = permissionService;
            _authenticationStateProvider = authenticationStateProvider;
        }

        public async Task AddArticleAsync(string title, string text, List<int> fileIds, ArticleType? eventType = null)
        {
            bool hasPermission = await _permissionService.HasPermissionAsync(EnumPermissions.AddArticle);
            if (!hasPermission) return;

            // Autor to zalogowany użytkownik – nie wpisuje się go już ręcznie
            var state = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var idClaim = state.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(idClaim, out var userId)) return;

            var user = await _authRepository.GetUserByIdAsync(userId);
            if (user == null) return;

            var fullName = $"{user.FirstName} {user.LastName}".Trim();

            var article = new Article
            {
                Title = title,
                Text = text,
                UserId = user.Id,
                Author = string.IsNullOrWhiteSpace(fullName) ? user.Login : fullName,
                PublishDate = DateTime.Now,
                FileId = fileIds.FirstOrDefault(),
                Images = fileIds
                    .Select((id, index) => new ArticleImage { FileId = id, Order = index })
                    .ToList(),
                EventsType = eventType
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
            if (id > 0)
            {
                await _articleRepository.RemoveArticleByIdAsync(id);
            }
        }

        // Edycja nie zmienia twórcy artykułu
        public async Task UpdateArticleByIdAsync(int id, string title, string text, int fileId,
            ArticleType? eventType = null)
        {
            var article = new Article
            {
                Id = id,
                Title = title,
                Text = text,
                FileId = fileId,
                EventsType = eventType
            };
            await _articleRepository.UpdateArticleByIdAsync(id, article);
        }
    }
}
