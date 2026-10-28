using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<ArticleService> _logger;

        public ArticleService(
            IArticleRepository articleRepository,
            IAuthRepository authRepository,
            IPermissionService permissionService,
            AuthenticationStateProvider authenticationStateProvider,
            ILogger<ArticleService> logger)
        {
            _articleRepository = articleRepository;
            _authRepository = authRepository;
            _permissionService = permissionService;
            _authenticationStateProvider = authenticationStateProvider;
            _logger = logger;
        }

        public async Task AddArticleAsync(string title, string text, List<int> fileIds, ArticleType? eventType = null)
        {
            // uprawnienia sprawdzamy tutaj, a nie tylko w interfejsie (brak uprawnień = wyjątek, a nie ciche pominięcie)
            await EnsureAsync(EnumPermissions.AddArticle);
            (title, text) = Validate(title, text, eventType);

            if (fileIds == null || fileIds.Count == 0)
            {
                throw new InvalidOperationException("Dodając artykuł, musisz dodać zdjęcie.");
            }

            // Autor to zalogowany użytkownik – nie wpisuje się go już ręcznie
            var state = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var idClaim = state.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(idClaim, out var userId))
            {
                throw new UnauthorizedAccessException();
            }

            var user = await _authRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new UnauthorizedAccessException();
            }

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
                throw new KeyNotFoundException($"Article with ID {id} not found.");
            }
            return article;
        }

        public async Task RemoveArticleByIdAsync(int id)
        {
            await EnsureAsync(EnumPermissions.DeleteArticle);

            if (id > 0)
            {
                await _articleRepository.RemoveArticleByIdAsync(id);
            }
        }

        // Edycja nie zmienia twórcy artykułu
        public async Task UpdateArticleByIdAsync(int id, string title, string text, int fileId,
            ArticleType? eventType = null)
        {
            await EnsureAsync(EnumPermissions.EditArticle);
            (title, text) = Validate(title, text, eventType);

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

        private async Task EnsureAsync(EnumPermissions permission)
        {
            if (!await _permissionService.HasPermissionAsync(permission))
            {
                throw new UnauthorizedAccessException();
            }
        }

        // Walidacja i czyszczenie – komunikaty trafiają bezpośrednio do użytkownika
        private static (string Title, string Text) Validate(string? title, string? text, ArticleType? eventType)
        {
            title = (title ?? string.Empty).Trim();
            text = (text ?? string.Empty).Trim();

            if (title.Length == 0)
            {
                throw new InvalidOperationException("Podaj tytuł artykułu.");
            }

            if (title.Length > ArticleContent.MaxTitleLength)
            {
                throw new InvalidOperationException(
                    $"Tytuł może mieć maksymalnie {ArticleContent.MaxTitleLength} znaków.");
            }

            if (ArticleContent.PlainText(text).Length == 0)
            {
                throw new InvalidOperationException("Wpisz treść artykułu.");
            }

            if (text.Length > ArticleContent.MaxTextLength)
            {
                throw new InvalidOperationException(
                    $"Treść może mieć maksymalnie {ArticleContent.MaxTextLength} znaków.");
            }

            if (eventType is ArticleType type && !Enum.IsDefined(type))
            {
                throw new InvalidOperationException("Nieznany typ artykułu.");
            }

            return (title, text);
        }
    }
}
