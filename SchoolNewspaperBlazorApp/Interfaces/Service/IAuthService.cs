namespace SchoolNewspaperBlazorApp.Interfaces.Service
{
    public interface IAuthService
    {
        Task AddUserAsync(string login, string password, string email);
        Task<int> CheckIfRegisteredAsync(string login, string password);
    }
}