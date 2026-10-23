using SchoolNewspaperBlazorApp.Data;

namespace SchoolNewspaperBlazorApp.Interfaces.Repository
{
    public interface IAuthRepository
    {
        Task AddUserAsync(User user);
        Task<User> CheckIfRegisteredAsync(string login, string password);
        Task<Dictionary<int, string>> GetAllUsersAsync();
        Task<User>? GetUserByIdAsync(int id);
    }
}
