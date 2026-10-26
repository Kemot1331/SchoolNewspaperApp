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
        public DbSet<SchoolClass> SchoolClasses { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Substitution> Substitutions { get; set; }
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

            // Słowniki zastępstw: każdy ma swoją tabelę i unikalną nazwę
            mb.Entity<SchoolClass>(e =>
            {
                e.ToTable("SchoolClasses");
                e.Property(x => x.Name).IsRequired().HasMaxLength(DictionaryKind.Class.MaxNameLength());
                e.HasIndex(x => x.Name).IsUnique();
            });
            mb.Entity<Teacher>(e =>
            {
                e.ToTable("Teachers");
                e.Property(x => x.Name).IsRequired().HasMaxLength(DictionaryKind.Teacher.MaxNameLength());
                e.HasIndex(x => x.Name).IsUnique();
            });
            mb.Entity<Room>(e =>
            {
                e.ToTable("Rooms");
                e.Property(x => x.Name).IsRequired().HasMaxLength(DictionaryKind.Room.MaxNameLength());
                e.HasIndex(x => x.Name).IsUnique();
            });
            mb.Entity<Subject>(e =>
            {
                e.ToTable("Subjects");
                e.Property(x => x.Name).IsRequired().HasMaxLength(DictionaryKind.Subject.MaxNameLength());
                e.HasIndex(x => x.Name).IsUnique();
            });

            // Zastępstwo -> słowniki. Restrict: wpisu słownika używanego w zastępstwach nie da się skasować
            // (SubstitutionService i tak sprawdza to wcześniej i podaje czytelny komunikat).
            mb.Entity<Substitution>(e =>
            {
                e.ToTable("Substitutions");
                e.Property(x => x.Note).IsRequired().HasMaxLength(Substitution.MaxNoteLength);
                e.HasIndex(x => x.Date);

                e.HasOne(x => x.Class).WithMany().HasForeignKey(x => x.ClassId)
                    .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Subject).WithMany().HasForeignKey(x => x.SubjectId)
                    .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.AbsentTeacher).WithMany().HasForeignKey(x => x.AbsentTeacherId)
                    .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.SubstituteTeacher).WithMany().HasForeignKey(x => x.SubstituteTeacherId)
                    .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Room).WithMany().HasForeignKey(x => x.RoomId)
                    .OnDelete(DeleteBehavior.Restrict);
                // po usunięciu konta zastępstwo zostaje (CreatedByUserId = NULL), tak jak artykuł
                e.HasOne(x => x.CreatedBy).WithMany().HasForeignKey(x => x.CreatedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);

                e.HasQueryFilter(x => !x.IsDeleted);
            });
        }
    }

}