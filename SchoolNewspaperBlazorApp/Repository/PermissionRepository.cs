using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SchoolNewspaperBlazorApp.Data;
using SchoolNewspaperBlazorApp.Interfaces.Repository;

namespace SchoolNewspaperBlazorApp.Repository;

public class PermissionRepository : IPermissionRepository
{
    private readonly IDbContextFactory<NewspaperDbContext> _factory;

    public PermissionRepository(IDbContextFactory<NewspaperDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<bool> HasPermissionAsync(ClaimsPrincipal principal, EnumPermissions permission)
    {

        var id = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(id))
        {
            return false;
        }
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
}