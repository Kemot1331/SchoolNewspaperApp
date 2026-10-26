using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<AuthService> _logger;

        private const int MinPasswordLength = 8;

        public AuthService(
            IAuthRepository authRepository,
            IPermissionRepository permissionRepository,
            IRoleRepository roleRepository,
            IPermissionService permissionService,
            AuthenticationStateProvider authenticationStateProvider,
            ILogger<AuthService> logger)
        {
            _authRepository = authRepository;
            _permissionRepository = permissionRepository;
            _roleRepository = roleRepository;
            _permissionService = permissionService;
            _authenticationStateProvider = authenticationStateProvider;
            _logger = logger;
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
                LastName = lastName.Trim(),
                // hasło ustawił admin, więc przy 1. logowaniu użytkownik ustawia własne
                MustChangePassword = true
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

        public async Task<UserDetails?> GetUserDetailsAsync(int id)
        {
            var user = await _authRepository.GetUserByIdAsync(id);
            return user == null
                ? null
                : new UserDetails(user.Id, user.Login, user.Email, user.FirstName, user.LastName);
        }

        public async Task UpdateUserAsync(int id, string login, string email, string firstName, string lastName, string? newPassword)
        {
            // 1. Tylko ManageUser lub Master – sprawdzamy to tutaj, a nie tylko w interfejsie
            var allowed = await _permissionService.HasPermissionAsync(EnumPermissions.ManageUser)
                          || await _permissionService.HasPermissionAsync(EnumPermissions.Master);
            if (!allowed)
            {
                throw new UnauthorizedAccessException();
            }

            login = (login ?? string.Empty).Trim();
            email = (email ?? string.Empty).Trim();
            firstName = (firstName ?? string.Empty).Trim();
            lastName = (lastName ?? string.Empty).Trim();

            if (login.Length == 0 || email.Length == 0)
            {
                throw new InvalidOperationException("Login i e-mail są wymagane.");
            }

            if (!email.Contains('@'))
            {
                throw new InvalidOperationException("Podaj poprawny adres e-mail.");
            }

            // 2. Konto administratora (Master) może edytować tylko jego właściciel
            //    (inaczej ktoś z ManageUser mógłby zmienić hasło Mastera i się na niego zalogować)
            var state = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var currentId = state.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentId != id.ToString())
            {
                var permissions = await _permissionRepository.GetUserPermissionsAsync(id);
                var role = await _roleRepository.GetRoleByUserIdAsync(id);
                if (permissions.Contains(EnumPermissions.Master) || Role.IsMasterName(role?.RoleName))
                {
                    throw new InvalidOperationException("Nie można edytować konta administratora.");
                }
            }

            // 3. Login i e-mail muszą być unikalne (po nich logujemy się do panelu)
            if (await _authRepository.IsLoginOrEmailTakenAsync(id, login, email))
            {
                throw new InvalidOperationException("Ten login lub e-mail jest już zajęty.");
            }

            // 4. Puste hasło = bez zmiany hasła
            string? passwordHash = null;
            if (!string.IsNullOrEmpty(newPassword))
            {
                passwordHash = new PasswordHasher<string>().HashPassword(null!, newPassword);
            }

            // Hasło ustawione komuś innemu = ta osoba musi je zmienić przy logowaniu.
            // Własne hasło zmieniamy świadomie, więc bez wymuszania.
            var forcePasswordChange = currentId != id.ToString();

            await _authRepository.UpdateUserAsync(id, login, email, firstName, lastName, passwordHash, forcePasswordChange);
        }

        public async Task<bool> MustChangePasswordAsync(int userId)
        {
            var user = await _authRepository.GetUserByIdAsync(userId);
            return user != null && user.MustChangePassword;
        }

        // userId pochodzi z ciasteczka logowania (nie z formularza), więc zmieniamy tylko własne hasło
        public async Task ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < MinPasswordLength)
            {
                throw new InvalidOperationException($"Nowe hasło musi mieć co najmniej {MinPasswordLength} znaków.");
            }

            var user = await _authRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("Konto nie istnieje.");
            }

            var hasher = new PasswordHasher<string>();
            var verification = hasher.VerifyHashedPassword(null!, user.Password, currentPassword ?? string.Empty);
            if (verification == PasswordVerificationResult.Failed)
            {
                throw new InvalidOperationException("Obecne hasło jest nieprawidłowe.");
            }

            if (currentPassword == newPassword)
            {
                throw new InvalidOperationException("Nowe hasło musi się różnić od obecnego.");
            }

            var newHash = hasher.HashPassword(null!, newPassword);
            await _authRepository.UpdatePasswordAsync(userId, newHash);
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