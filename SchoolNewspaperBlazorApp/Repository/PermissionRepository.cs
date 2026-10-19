using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
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

    public Task<bool> HasPermissionAsync(ClaimsPrincipal principal, EnumPermissions permission)
    {
        var id = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}