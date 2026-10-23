using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SchoolNewspaperBlazorApp.Data;
using SchoolNewspaperBlazorApp.Interfaces.Repository;
using SchoolNewspaperBlazorApp.Interfaces.Service;

namespace SchoolNewspaperBlazorApp.Service
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IPermissionRepository _permissionRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IPermissionService _permissionService;
        private readonly AuthenticationStateProvider _authenticationStateProvider;

        public AuthService(
            IAuthRepository authRepository,
            IPermissionRepository permissionRepository,
            IRoleRepository roleRepository,
            IPermissionService permissionService,
            AuthenticationStateProvider authenticationStateProvider)
        {
            _authRepository = authRepository;
            _permissionRepository = permissionRepository;
            _roleRepository = roleRepository;
            _permissionService = permissionService;
            _authenticationStateProvider = authenticationStateProvider;
        }
        
        public async Task<Dictionary<int, string>> GetAllUsersAsync()
        {
            return await _authRepository.GetAllUsersAsync();
        }

        public async Task AddUserAsync(string login, string password, string email, string firstName, string lastName)
        {
            var hasher = new PasswordHasher<string>();
            string passwordHash = hasher.HashPassword(null!, password);
            var user = new User
            {
                Login = login,
                Password = passwordHash,
                Email = email,
                RegisterDate = DateTime.Now,
                FirstName = firstName.Trim(),
                LastName = lastName.Trim()
            };

            await _authRepository.AddUserAsync(user);
        }
        public async Task<int> CheckIfRegisteredAsync(
            string login, string password)
        {
            var hasher = new PasswordHasher<string>();
            // Uwaga: hasła powinny być przechowywane w postaci skrótu (hash) — to tylko przykład działania.
            try
            {
                var user = await _authRepository.CheckIfRegisteredAsync(login, password);
                var result = hasher.VerifyHashedPassword(login, user.Password, password);
                if (result == PasswordVerificationResult.Success)
                {
                    return user.Id;
                }
                else {
                    return 0;
                }
            }
            catch (Exception) 
            {
                return 0;
            }
        }

        public async Task<string> GetFullNameAsync(int userId)
        {
            var user = await _authRepository.GetUserByIdAsync(userId);
            return user == null ? "" : $"{user.FirstName} {user.LastName}".Trim();
        }

        public async Task DeleteUserByIdAsync(int id)
        {
            // 1. Tylko ManageUser lub Master – sprawdzamy to tutaj, a nie tylko w interfejsie
            var allowed = await _permissionService.HasPermissionAsync(EnumPermissions.ManageUser)
                          || await _permissionService.HasPermissionAsync(EnumPermissions.Master);
            if (!allowed)
            {
                throw new UnauthorizedAccessException();
            }

            // 2. Nie usuwamy własnego konta
            var state = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var currentId = state.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentId == id.ToString())
            {
                throw new InvalidOperationException("Nie możesz usunąć własnego konta.");
            }

            // 3. Nie usuwamy konta administratora (Master)
            var permissions = await _permissionRepository.GetUserPermissionsAsync(id);
            var role = await _roleRepository.GetRoleByUserIdAsync(id);
            if (permissions.Contains(EnumPermissions.Master) || Role.IsMasterName(role?.RoleName))
            {
                throw new InvalidOperationException("Nie można usunąć konta administratora.");
            }

            await _authRepository.DeleteUserByIdAsync(id);
        }
    }
}