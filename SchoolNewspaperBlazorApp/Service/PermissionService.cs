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

    public Task<List<EnumPermissions>> GetUserPermissionsAsync(int userId)
    {
        return _permissionRepository.GetUserPermissionsAsync(userId);
    }

    public Task SetUserPermissionsAsync(int userId, IEnumerable<EnumPermissions> permissions)
    {
        // TODO: dodać sprawdzanie ManageUser / Master, gdy uprawnienia będą już nadawane
        return _permissionRepository.SetUserPermissionsAsync(userId, permissions);
    }
}