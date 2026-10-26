using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using SchoolNewspaperBlazorApp.Data;
using SchoolNewspaperBlazorApp.Interfaces.Repository;
using SchoolNewspaperBlazorApp.Interfaces.Service;

namespace SchoolNewspaperBlazorApp.Service;

public class PermissionService : IPermissionService
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly ILogger<PermissionService> _logger;

    public PermissionService(
        IPermissionRepository permissionRepository,
        AuthenticationStateProvider authenticationStateProvider,
        ILogger<PermissionService> logger)
    {
        _permissionRepository = permissionRepository;
        _authenticationStateProvider = authenticationStateProvider;
        _logger = logger;
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

    // true, jeśli konto ma choć jedno uprawnienie (np. do wyświetlenia linku do panelu)
    public async Task<bool> HasAnyPermissionAsync()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var principal = authState.User;

        if (principal.Identity?.IsAuthenticated != true
            || !int.TryParse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
        {
            return false;
        }

        var permissions = await _permissionRepository.GetUserPermissionsAsync(userId);
        return permissions.Count > 0;
    }

    public Task<List<EnumPermissions>> GetUserPermissionsAsync(int userId)
    {
        return _permissionRepository.GetUserPermissionsAsync(userId);
    }

    public async Task SetUserPermissionsAsync(int userId, IEnumerable<EnumPermissions> permissions)
    {
        // Tylko ManageUser lub Master – sprawdzamy tutaj, a nie tylko w interfejsie
        var allowed = await HasPermissionAsync(EnumPermissions.ManageUser)
                      || await HasPermissionAsync(EnumPermissions.Master);
        if (!allowed)
        {
            throw new UnauthorizedAccessException();
        }

        await _permissionRepository.SetUserPermissionsAsync(userId, permissions);
    }
}