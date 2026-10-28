using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchoolNewspaperBlazorApp.Data;
using SchoolNewspaperBlazorApp.Interfaces.Repository;

namespace SchoolNewspaperBlazorApp.Repository
{
    public class AuthRepository : IAuthRepository
    {
        private readonly IDbContextFactory<NewspaperDbContext> _factory;
        private readonly ILogger<AuthRepository> _logger;

        public AuthRepository(IDbContextFactory<NewspaperDbContext> factory, ILogger<AuthRepository> logger)
        {
            _factory = factory;
            _logger = logger;
        }

        public async Task<Dictionary<int, string>> GetAllUsersAsync()
        {
            await using var context = await _factory.CreateDbContextAsync();
            return await context.User.ToDictionaryAsync(u => u.Id, u => u.Email);
        }

        public async Task<User>? GetUserByIdAsync(int id)
        {
            await using var context = await _factory.CreateDbContextAsync();

            return await context.User.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task DeleteUserByIdAsync(int id)
        {
            await using var context = await _factory.CreateDbContextAsync();
            // filtr soft delete pomija już usuniętych, więc drugi raz konta nie "usuniemy"
            var user = await context.User.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                throw new Exception("Konto nie istnieje");
            }
            user.IsDeleted = true;
            await context.SaveChangesAsync();
        }

        public async Task AddUserAsync(User user)
        {
            await using var context = await _factory.CreateDbContextAsync();
            await context.User.AddAsync(user);
            await context.SaveChangesAsync();
        }

        // Sprawdza też "na krzyż" (login jednego konta = e-mail drugiego), bo logować można się i loginem, i e-mailem –
        // inaczej dwa różne konta mogłyby odpowiadać na ten sam wpisany tekst.
        public async Task<bool> IsLoginOrEmailTakenAsync(int excludeUserId, string login, string email)
        {
            await using var context = await _factory.CreateDbContextAsync();
            return await context.User.AnyAsync(u =>
                u.Id != excludeUserId
                && (u.Login == login || u.Email == email || u.Login == email || u.Email == login));
        }

        // passwordHash == null oznacza: hasła nie zmieniamy
        // forcePasswordChange: po zmianie hasła przez admina użytkownik musi ustawić własne
        public async Task UpdateUserAsync(int id, string login, string email, string firstName, string lastName, string? passwordHash, bool forcePasswordChange)
        {
            await using var context = await _factory.CreateDbContextAsync();
            var user = await context.User.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                throw new Exception("Konto nie istnieje");
            }

            user.Login = login;
            user.Email = email;
            user.FirstName = firstName;
            user.LastName = lastName;
            if (passwordHash != null)
            {
                user.Password = passwordHash;
                user.MustChangePassword = forcePasswordChange;
            }

            await context.SaveChangesAsync();
        }

        // Zapisuje nowe hasło i zdejmuje wymuszenie jego zmiany
        public async Task UpdatePasswordAsync(int id, string passwordHash)
        {
            await using var context = await _factory.CreateDbContextAsync();
            var user = await context.User.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                throw new Exception("Konto nie istnieje");
            }

            user.Password = passwordHash;
            user.MustChangePassword = false;
            await context.SaveChangesAsync();
        }

        // Zwraca użytkownika znalezionego po loginie LUB e-mailu (hasło sprawdza AuthService).
        public async Task<User> CheckIfRegisteredAsync(string login, string password)
        {
            await using var context = await _factory.CreateDbContextAsync();
            var user = await context.User
                .AsNoTracking()
                .OrderBy(u => u.Id)
                .FirstOrDefaultAsync(u => u.Login == login || u.Email == login);

            if (user == null)
            {
                throw new Exception("Konto nie istnieje");
            }

            return user;
        }
    }
}
