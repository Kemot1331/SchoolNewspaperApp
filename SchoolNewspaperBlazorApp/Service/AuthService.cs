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
            var user = new User
            {
                Login = login,
                Password = password,
                Email = email,
                RegisterDate = DateTime.Now
            };
            await _authRepository.AddUserAsync(user);
        }
    }
}
