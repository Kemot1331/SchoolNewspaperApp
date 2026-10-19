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
}