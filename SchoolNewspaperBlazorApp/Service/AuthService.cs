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

        public AuthService(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }


        public async Task AddUserAsync(string login, string password, string email)
        {
            var hasher = new PasswordHasher<string>();
            string passwordHash = hasher.HashPassword(null!, password);
            var user = new User
            {
                Login = login,
                Password = passwordHash,
                Email = email,
                RegisterDate = DateTime.Now
            };

            await _authRepository.AddUserAsync(user);
        }
        public async Task<bool> CheckIfRegisteredAsync(
            string login, string password)
        {
            var hasher = new PasswordHasher<string>();
            // Uwaga: hasła powinny być przechowywane w postaci skrótu (hash) — to tylko przykład działania.
            try
            {
                var dbPass = await _authRepository.CheckIfRegisteredAsync(login, password);
                var result = hasher.VerifyHashedPassword(login, dbPass, password);
                if (result == PasswordVerificationResult.Success)
                {
                    return true;
                }
                else {
                    return false;
                }
            }
            catch (Exception) 
            {
                return false;
            }
        }
    }
}