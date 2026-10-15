namespace SchoolNewspaperBlazorApp.Interfaces.Service
{
    public interface IAuthService
    {
        Task AddUserAsync(string login, string password, string email);
        Task<bool> CheckIfRegisteredAsync(string login, string password);
    }
}