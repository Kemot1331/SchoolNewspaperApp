using Microsoft.EntityFrameworkCore;
using SchoolNewspaperBlazorApp.Data;
using SchoolNewspaperBlazorApp.Interfaces.Repository;

namespace SchoolNewspaperBlazorApp.Repository
{
    public class RoleRepository : IRoleRepository
    {
        private readonly IDbContextFactory<NewspaperDbContext> _factory;

        public RoleRepository(IDbContextFactory<NewspaperDbContext> factory)
        {
            _factory = factory;
        }

        public async Task<Dictionary<int, string>> GetAllRolesAsync()
        {
            await using var context = await _factory.CreateDbContextAsync();
            return await context.Roles.AsNoTracking().ToDictionaryAsync(r => r.UserId, r => r.RoleName);
        }

        public async Task<Role?> GetRoleByUserIdAsync(int userId)
        {
            await using var context = await _factory.CreateDbContextAsync();
            return await context.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.UserId == userId);
        }

        // 1:1 – użytkownik ma jedną rolę: jeśli ją ma, zmieniamy nazwę, jeśli nie, tworzymy
        public async Task SetRoleAsync(int userId, string roleName)
        {
            await using var context = await _factory.CreateDbContextAsync();

            if (!await context.User.AnyAsync(u => u.Id == userId))
            {
                throw new Exception("Konto nie istnieje");
            }

            var role = await context.Roles.FirstOrDefaultAsync(r => r.UserId == userId);
            if (role == null)
            {
                await context.Roles.AddAsync(new Role { UserId = userId, RoleName = roleName });
            }
            else
            {
                role.RoleName = roleName;
            }

            await context.SaveChangesAsync();
        }
    }
}
