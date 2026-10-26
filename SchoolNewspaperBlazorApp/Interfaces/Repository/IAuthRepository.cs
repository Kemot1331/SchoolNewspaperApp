using SchoolNewspaperBlazorApp.Data;

namespace SchoolNewspaperBlazorApp.Interfaces.Repository
{
    public interface IAuthRepository
    {
        Task AddUserAsync(User user);
        Task<User> CheckIfRegisteredAsync(string login, string password);
        Task<Dictionary<int, string>> GetAllUsersAsync();
        Task<User>? GetUserByIdAsync(int id);
        Task DeleteUserByIdAsync(int id);
        Task<bool> IsLoginOrEmailTakenAsync(int excludeUserId, string login, string email);
        Task UpdateUserAsync(int id, string login, string email, string firstName, string lastName, string? passwordHash, bool forcePasswordChange);
        Task UpdatePasswordAsync(int id, string passwordHash);
    }
}