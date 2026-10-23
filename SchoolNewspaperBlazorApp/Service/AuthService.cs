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

        public AuthService(IAuthRepository authRepository,  IPermissionRepository permissionRepository)
        {
            _authRepository = authRepository;
            _permissionRepository = permissionRepository;
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
            return user.FirstName + " " + user.LastName;
        }
    }
}