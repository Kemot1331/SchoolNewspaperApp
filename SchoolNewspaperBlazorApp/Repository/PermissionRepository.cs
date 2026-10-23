using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using SchoolNewspaperBlazorApp.Data;
using SchoolNewspaperBlazorApp.Interfaces.Repository;

namespace SchoolNewspaperBlazorApp.Repository;

public class PermissionRepository : IPermissionRepository
{
    private readonly IDbContextFactory<NewspaperDbContext> _factory;
    private readonly ILogger<PermissionRepository> _logger;

    public PermissionRepository(IDbContextFactory<NewspaperDbContext> factory, ILogger<PermissionRepository> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public async Task<bool> HasPermissionAsync(ClaimsPrincipal principal, EnumPermissions permission)
    {
        var id = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (id.IsNullOrEmpty())
        {
            return false;
        }

        if (!int.TryParse(id, out var permissionId))
        {
            return false;
        }
        await using var context = await _factory.CreateDbContextAsync();

        return await context.UserPermissions
            .AnyAsync(up => up.UserId == permissionId && up.Permission.PermissionsName == permission);
    }

    public async Task<List<EnumPermissions>> GetUserPermissionsAsync(int userId)
    {
        await using var context = await _factory.CreateDbContextAsync();

        return await context.UserPermissions
            .Where(up => up.UserId == userId)
            .Select(up => up.Permission.PermissionsName)
            .ToListAsync();
    }

    public async Task SetUserPermissionsAsync(int userId, IEnumerable<EnumPermissions> permissions)
    {
        await using var context = await _factory.CreateDbContextAsync();

        // Master nigdy nie jest zmieniane z panelu
        var wanted = permissions
            .Where(p => p != EnumPermissions.Master)
            .Distinct()
            .ToList();

        var existing = await context.UserPermissions
            .Include(up => up.Permission)
            .Where(up => up.UserId == userId)
            .ToListAsync();

        var toRemove = existing
            .Where(up => up.Permission.PermissionsName != EnumPermissions.Master
                         && !wanted.Contains(up.Permission.PermissionsName))
            .ToList();
        context.UserPermissions.RemoveRange(toRemove);

        var have = existing.Select(up => up.Permission.PermissionsName).ToHashSet();
        var toAdd = wanted.Where(p => !have.Contains(p)).ToList();

        if (toAdd.Count > 0)
        {
            var rows = await context.Permissions
                .Where(p => toAdd.Contains(p.PermissionsName))
                .ToListAsync();

            foreach (var permission in toAdd)
            {
                var row = rows.FirstOrDefault(r => r.PermissionsName == permission);
                if (row == null)
                {
                    row = new Permission
                    {
                        PermissionsName = permission,
                        PermissionViewName = permission.ToString()
                    };
                    context.Permissions.Add(row);
                }

                context.UserPermissions.Add(new UserPermission
                {
                    UserId = userId,
                    Permission = row,
                    PermissionName = permission.ToString()
                });
            }
        }

        await context.SaveChangesAsync();
    }
}