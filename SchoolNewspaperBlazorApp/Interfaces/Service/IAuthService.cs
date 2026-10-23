namespace SchoolNewspaperBlazorApp.Interfaces.Service
{
    public interface IAuthService
    {
        Task AddUserAsync(string login, string password, string email, string firstName, string lastName);
        Task<Dictionary<int, string>> GetAllUsersAsync();
        Task<int> CheckIfRegisteredAsync(string login, string password);
        Task<string> GetFullNameAsync(int userId);
    }
}