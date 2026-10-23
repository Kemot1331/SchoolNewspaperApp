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
                await using var context = _factory.CreateDbContext();

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

            public async Task<bool> IsLoginOrEmailTakenAsync(int excludeUserId, string login, string email)
            {
                await using var context = await _factory.CreateDbContextAsync();
                return await context.User.AnyAsync(u =>
                    u.Id != excludeUserId && (u.Login == login || u.Email == email));
            }

            // passwordHash == null oznacza: hasła nie zmieniamy
            public async Task UpdateUserAsync(int id, string login, string email, string firstName, string lastName, string? passwordHash)
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
                }

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