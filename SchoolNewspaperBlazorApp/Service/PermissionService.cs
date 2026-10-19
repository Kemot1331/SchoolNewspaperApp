using Microsoft.AspNetCore.Components.Authorization;
using SchoolNewspaperBlazorApp.Data;
using SchoolNewspaperBlazorApp.Interfaces.Repository;
using SchoolNewspaperBlazorApp.Interfaces.Service;

namespace SchoolNewspaperBlazorApp.Service;

public class PermissionService : IPermissionService
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public PermissionService(
        IPermissionRepository permissionRepository,
        AuthenticationStateProvider authenticationStateProvider)
    {
        _permissionRepository = permissionRepository;
        _authenticationStateProvider = authenticationStateProvider;
    }

    public async Task<bool> HasPermissionAsync(EnumPermissions permission)
    {
        var authState = await _authenticationStateProvider
            .GetAuthenticationStateAsync();

        var principal = authState.User;

        if (principal.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        return await _permissionRepository
            .HasPermissionAsync(principal, permission);
    }
}