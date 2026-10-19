using System.Security.Claims;
using SchoolNewspaperBlazorApp.Data;

namespace SchoolNewspaperBlazorApp.Interfaces.Repository;

public interface IPermissionRepository
{
    Task<bool> HasPermissionAsync(ClaimsPrincipal principal, EnumPermissions permission);
}