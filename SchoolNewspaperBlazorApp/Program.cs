using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using SchoolNewspaperBlazorApp.Components;
using SchoolNewspaperBlazorApp.Data;
using SchoolNewspaperBlazorApp.Interfaces.Repository;
using SchoolNewspaperBlazorApp.Interfaces.Service;
using SchoolNewspaperBlazorApp.Repository;
using SchoolNewspaperBlazorApp.Service;
using System.Security.Claims;

namespace SchoolNewspaperBlazorApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();
            //sql connect
            builder.Services.AddDbContextFactory<NewspaperDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("NewspaperConnectionString")));

            //Repositry
            builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
            builder.Services.AddScoped<IAuthRepository, AuthRepository>();
            builder.Services.AddScoped<IFileRepository, FileRepository>();
            builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
            builder.Services.AddScoped<IRoleRepository, RoleRepository>();
            builder.Services.AddScoped<IDictionaryRepository, DictionaryRepository>();
            builder.Services.AddScoped<ISubstitutionRepository, SubstitutionRepository>();


            //Services
            builder.Services.AddScoped<IArticleService, ArticleService>();
            builder.Services.AddScoped<IFileService, FileService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IPermissionService, PermissionService>();
            builder.Services.AddScoped<IRoleService, RoleService>();
            builder.Services.AddScoped<ToastService>();
            builder.Services.AddScoped<IDictionaryService, DictionaryService>();
            builder.Services.AddScoped<ISubstitutionService, SubstitutionService>();

            builder.Services.AddAuthentication(
                CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/login";
                    options.Cookie.Name = "SchoolNewspaper.Auth";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    options.ExpireTimeSpan = TimeSpan.FromHours(1);
                    options.SlidingExpiration = true;
                });
            builder.Services.AddAuthorization();
            builder.Services.AddCascadingAuthenticationState();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
               
                app.UseHsts();
            }
            app.MapPost("/auth/login", async (
                HttpContext context,
                IAuthService authService) =>
            {
                var form = await context.Request.ReadFormAsync();
                string email = form["Email"].ToString();
                string password = form["Password"].ToString();
                int userId =  await authService.CheckIfRegisteredAsync(email, password);
                if (userId == 0)
                {
                    return Results.Redirect("/");
                }

                // imię i nazwisko pokazujemy w nagłówku obok "Wyloguj się"
                var fullName = await authService.GetFullNameAsync(userId);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Name,email)
                };
                if (!string.IsNullOrWhiteSpace(fullName))
                {
                    claims.Add(new Claim("FullName", fullName));
                }

                // 1. logowanie (albo reset hasła przez admina) – najpierw trzeba ustawić własne hasło
                var mustChangePassword = await authService.MustChangePasswordAsync(userId);
                if (mustChangePassword)
                {
                    claims.Add(new Claim("MustChangePassword", "true"));
                }

                var identity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    principal);
                return Results.Redirect(mustChangePassword ? "/zmiana-hasla" : "/adminPanel");
            });

            // zmiana własnego hasła (formularz ze strony /zmiana-hasla)
            app.MapPost("/auth/change-password", async (
                HttpContext context,
                IAuthService authService) =>
            {
                if (context.User.Identity?.IsAuthenticated != true
                    || !int.TryParse(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
                {
                    return Results.Redirect("/login");
                }

                var form = await context.Request.ReadFormAsync();
                string currentPassword = form["CurrentPassword"].ToString();
                string newPassword = form["NewPassword"].ToString();
                string confirmPassword = form["ConfirmPassword"].ToString();

                if (newPassword != confirmPassword)
                {
                    return Results.Redirect("/zmiana-hasla?error=" + Uri.EscapeDataString("Nowe hasła nie są takie same."));
                }

                try
                {
                    await authService.ChangePasswordAsync(userId, currentPassword, newPassword);
                }
                catch (InvalidOperationException ex)
                {
                    return Results.Redirect("/zmiana-hasla?error=" + Uri.EscapeDataString(ex.Message));
                }
                catch (Exception)
                {
                    return Results.Redirect("/zmiana-hasla?error=" + Uri.EscapeDataString("Nie udało się zmienić hasła."));
                }

                // nowe ciasteczko bez znacznika MustChangePassword, inaczej middleware dalej blokowałby użytkownika
                var claims = context.User.Claims.Where(c => c.Type != "MustChangePassword").ToList();
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity));
                return Results.Redirect("/adminPanel");
            });
            
            //wylogowanie sie
            app.MapPost("/auth/logout", async (HttpContext context) =>
            {
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return Results.Redirect("/");
            });
            
            var imagesPath = Path.Combine(
                builder.Environment.ContentRootPath,
                "Images"
            );
            Directory.CreateDirectory(imagesPath);
            app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(imagesPath),
                RequestPath = "/Images"
            });

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            // Kto musi zmienić hasło, widzi tylko stronę zmiany hasła (nie wejdzie ręcznie np. na /adminPanel)
            app.UseAuthentication();
            app.Use(async (context, next) =>
            {
                var path = context.Request.Path;
                var mustChange = context.User.Identity?.IsAuthenticated == true
                                 && context.User.HasClaim("MustChangePassword", "true");
                var allowed = path.StartsWithSegments("/zmiana-hasla")
                              || path.StartsWithSegments("/auth")
                              || path.StartsWithSegments("/_blazor")
                              || path.StartsWithSegments("/_framework")
                              || path.StartsWithSegments("/_content");

                if (mustChange && !allowed)
                {
                    context.Response.Redirect("/zmiana-hasla");
                    return;
                }

                await next();
            });

            // musi być po UseAuthentication, inaczej [Authorize] widzi użytkownika jako niezalogowanego
            app.UseAuthorization();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<string>();
            Console.WriteLine(hasher.HashPassword(null!, "admin123"));
            
            app.Run();
        }
    }
}