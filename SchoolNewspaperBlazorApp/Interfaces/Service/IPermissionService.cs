using System.Security.Claims;
using SchoolNewspaperBlazorApp.Data;

namespace SchoolNewspaperBlazorApp.Interfaces.Service;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(EnumPermissions permission);
}