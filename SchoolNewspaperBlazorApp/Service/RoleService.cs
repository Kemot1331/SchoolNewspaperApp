using Microsoft.Extensions.Logging;
using SchoolNewspaperBlazorApp.Data;
using SchoolNewspaperBlazorApp.Interfaces.Repository;
using SchoolNewspaperBlazorApp.Interfaces.Service;

namespace SchoolNewspaperBlazorApp.Service
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IPermissionService _permissionService;
        private readonly ILogger<RoleService> _logger;

        public RoleService(IRoleRepository roleRepository, IPermissionService permissionService, ILogger<RoleService> logger)
        {
            _roleRepository = roleRepository;
            _permissionService = permissionService;
            _logger = logger;
        }

        public Task<Dictionary<int, string>> GetAllRolesAsync()
        {
            return _roleRepository.GetAllRolesAsync();
        }

        public async Task SetRoleAsync(int userId, string roleName)
        {
            // Tylko ManageUser lub Master – sprawdzamy tutaj, a nie tylko w interfejsie
            var allowed = await _permissionService.HasPermissionAsync(EnumPermissions.ManageUser)
                          || await _permissionService.HasPermissionAsync(EnumPermissions.Master);
            if (!allowed)
            {
                throw new UnauthorizedAccessException();
            }

            roleName = (roleName ?? string.Empty).Trim();
            if (roleName.Length == 0)
            {
                throw new InvalidOperationException("Podaj nazwę roli.");
            }

            if (roleName.Length > Role.MaxNameLength)
            {
                throw new InvalidOperationException($"Nazwa roli może mieć maksymalnie {Role.MaxNameLength} znaków.");
            }

            // Roli Master nie da się nadać z aplikacji (tylko SQL)
            if (Role.IsMasterName(roleName))
            {
                throw new InvalidOperationException("Roli Master nie można nadać z panelu.");
            }

            // ...i nie da się jej edytować, jeśli ktoś już ją ma
            var current = await _roleRepository.GetRoleByUserIdAsync(userId);
            if (current != null && Role.IsMasterName(current.RoleName))
            {
                throw new InvalidOperationException("Roli Master nie można zmienić ani usunąć.");
            }

            await _roleRepository.SetRoleAsync(userId, roleName);
        }
    }
}