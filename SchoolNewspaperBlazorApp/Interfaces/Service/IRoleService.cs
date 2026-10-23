namespace SchoolNewspaperBlazorApp.Interfaces.Service
{
    public interface IRoleService
    {
        Task<Dictionary<int, string>> GetAllRolesAsync();
        Task SetRoleAsync(int userId, string roleName);
    }
}
