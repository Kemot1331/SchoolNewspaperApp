using SchoolNewspaperBlazorApp.Data;

namespace SchoolNewspaperBlazorApp.Interfaces.Repository
{
    public interface IAuthRepository
    {
        Task AddUserAsync(User user);
        Task<User> CheckIfRegisteredAsync(string login, string password);
        Task<List<String>> GetAllUsersAsync();
    }
}