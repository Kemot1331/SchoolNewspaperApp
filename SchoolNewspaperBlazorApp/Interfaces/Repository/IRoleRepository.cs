using SchoolNewspaperBlazorApp.Data;

namespace SchoolNewspaperBlazorApp.Interfaces.Repository
{
    public interface IRoleRepository
    {
        // userId -> nazwa roli
        Task<Dictionary<int, string>> GetAllRolesAsync();
        Task<Role?> GetRoleByUserIdAsync(int userId);
        Task SetRoleAsync(int userId, string roleName);
    }
}
