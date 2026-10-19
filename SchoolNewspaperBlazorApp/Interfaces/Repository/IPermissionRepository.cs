using System.Security.Claims;
using SchoolNewspaperBlazorApp.Data;

namespace SchoolNewspaperBlazorApp.Interfaces.Repository;

public interface IPermissionRepository
{
    Task<bool> HasPermissionAsync(ClaimsPrincipal principal, EnumPermissions permission);
    Task<List<EnumPermissions>> GetUserPermissionsAsync(int userId);
    Task SetUserPermissionsAsync(int userId, IEnumerable<EnumPermissions> permissions);
}