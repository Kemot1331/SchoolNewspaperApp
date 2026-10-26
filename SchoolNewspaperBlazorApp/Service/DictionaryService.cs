using SchoolNewspaperBlazorApp.Data;
using SchoolNewspaperBlazorApp.Interfaces.Repository;
using SchoolNewspaperBlazorApp.Interfaces.Service;

namespace SchoolNewspaperBlazorApp.Service
{
    public class DictionaryService : IDictionaryService
    {
        private readonly IDictionaryRepository _repository;
        private readonly IPermissionService _permissionService;
        private readonly ILogger<DictionaryService> _logger;

        public DictionaryService(
            IDictionaryRepository repository,
            IPermissionService permissionService,
            ILogger<DictionaryService> logger)
        {
            _repository = repository;
            _permissionService = permissionService;
            _logger = logger;
        }

        // Lista nie wymaga uprawnień – te same nazwy i tak widać publicznie w zastępstwach
        public Task<List<DictionaryRow>> GetAllAsync(DictionaryKind kind) => _repository.GetAllAsync(kind);

        // Uprawnienie danego słownika (np. ManageTeachers) albo Master
        public async Task<bool> CanManageAsync(DictionaryKind kind)
        {
            return await _permissionService.HasPermissionAsync(kind.RequiredPermission())
                   || await _permissionService.HasPermissionAsync(EnumPermissions.Master);
        }

        public async Task AddAsync(DictionaryKind kind, string name)
        {
            await EnsureCanManageAsync(kind);
            name = ValidateName(kind, name);

            if (await _repository.IsNameTakenAsync(kind, 0, name))
            {
                throw new InvalidOperationException($"{kind.ToSingular()} „{name}” już istnieje.");
            }

            await _repository.AddAsync(kind, name);
        }

        public async Task UpdateAsync(DictionaryKind kind, int id, string name)
        {
            await EnsureCanManageAsync(kind);
            name = ValidateName(kind, name);

            if (await _repository.IsNameTakenAsync(kind, id, name))
            {
                throw new InvalidOperationException($"{kind.ToSingular()} „{name}” już istnieje.");
            }

            await _repository.UpdateAsync(kind, id, name);
        }

        public async Task DeleteAsync(DictionaryKind kind, int id)
        {
            await EnsureCanManageAsync(kind);

            if (await _repository.IsInUseAsync(kind, id))
            {
                throw new InvalidOperationException(
                    "Ten wpis jest użyty w zastępstwach, więc nie można go usunąć. Możesz zmienić jego nazwę.");
            }

            await _repository.DeleteAsync(kind, id);
        }

        private async Task EnsureCanManageAsync(DictionaryKind kind)
        {
            // sprawdzamy tutaj, a nie tylko w interfejsie
            if (!await CanManageAsync(kind))
            {
                throw new UnauthorizedAccessException();
            }
        }

        private static string ValidateName(DictionaryKind kind, string? name)
        {
            name = (name ?? string.Empty).Trim();

            if (name.Length == 0)
            {
                throw new InvalidOperationException("Podaj nazwę.");
            }

            var max = kind.MaxNameLength();
            if (name.Length > max)
            {
                throw new InvalidOperationException($"Nazwa może mieć maksymalnie {max} znaków.");
            }

            return name;
        }
    }
}
