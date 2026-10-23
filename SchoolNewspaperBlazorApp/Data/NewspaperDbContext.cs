using Microsoft.EntityFrameworkCore;

namespace SchoolNewspaperBlazorApp.Data
{
    public class NewspaperDbContext : DbContext
    {
        public NewspaperDbContext(DbContextOptions<NewspaperDbContext> options)
            : base(options) {}
        public DbSet<Article> Articles { get; set; }
        public DbSet<MediaFile> Files { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<ArticleImage> Images { get; set; }
        public DbSet<Role> Roles { get; set; }
        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);

            mb.Entity<MediaFile>()
                .Property(x => x.Id)
                .UseIdentityColumn();
            mb.Entity<UserPermission>()
                .HasKey(x => new { x.UserId, x.PermissionId });
            mb.Entity<UserPermission>()
                .HasOne(x => x.User)
                .WithMany(x => x.UserPermissions)
                .HasForeignKey(x => x.UserId);
            mb.Entity<UserPermission>()
                .HasOne(x => x.Permission)
                .WithMany(x => x.Permissions)
                .HasForeignKey(x => x.PermissionId);

            // User 1:1 Role (klucz obcy UserId leży w Role; usunięcie użytkownika usuwa jego rolę)
            mb.Entity<Role>()
                .HasOne(r => r.User)
                .WithOne(u => u.Role)
                .HasForeignKey<Role>(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            mb.Entity<Role>()
                .Property(r => r.RoleName)
                .IsRequired()
                .HasMaxLength(Role.MaxNameLength);

            // Artykuł -> twórca; po usunięciu konta artykuł zostaje (UserId = NULL)
            mb.Entity<Article>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Soft delete: usunięte wiersze znikają ze wszystkich zapytań (Include też).
            // Zapytanie, które ma je zobaczyć, dopisuje .IgnoreQueryFilters().
            mb.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
            mb.Entity<Article>().HasQueryFilter(a => !a.IsDeleted);
            // Rola i uprawnienia usuniętego użytkownika też przestają obowiązywać
            mb.Entity<Role>().HasQueryFilter(r => !r.User.IsDeleted);
            mb.Entity<UserPermission>().HasQueryFilter(up => !up.User.IsDeleted);
        }
    }

}