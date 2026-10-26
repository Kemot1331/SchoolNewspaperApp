using System.Security.Claims;
using SchoolNewspaperBlazorApp.Data;

namespace SchoolNewspaperBlazorApp.Interfaces.Service;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(EnumPermissions permission);
    Task<bool> HasAnyPermissionAsync();
    Task<List<EnumPermissions>> GetUserPermissionsAsync(int userId);
    Task SetUserPermissionsAsync(int userId, IEnumerable<EnumPermissions> permissions);
}