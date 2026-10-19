using Microsoft.EntityFrameworkCore;
using SchoolNewspaperBlazorApp.Data;
using SchoolNewspaperBlazorApp.Interfaces.Repository;

namespace SchoolNewspaperBlazorApp.Repository
{
    public class AuthRepository : IAuthRepository
    {
        private readonly IDbContextFactory<NewspaperDbContext> _factory;

        public AuthRepository(IDbContextFactory<NewspaperDbContext> factory)
        {
            _factory = factory;
        }

        public async Task<Dictionary<int, string>> GetAllUsersAsync()
        {
            await using var context = await _factory.CreateDbContextAsync();
            return await context.User.ToDictionaryAsync(u => u.Id, u => u.Email);
        }

        public async Task AddUserAsync(User user)
        {
            await using var context = await _factory.CreateDbContextAsync();
            await context.User.AddAsync(user);
            await context.SaveChangesAsync();
        }

        // Zwraca hash hasła użytkownika znalezionego po loginie LUB e-mailu.
        public async Task<User> CheckIfRegisteredAsync(string login, string password)
        {
            await using var context = await _factory.CreateDbContextAsync();
            var user = await context.User
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Login == login || u.Email == login);

            if (user == null)
            {
                throw new Exception("Konto nie istnieje");
            }

            return user;
        }
    }
}