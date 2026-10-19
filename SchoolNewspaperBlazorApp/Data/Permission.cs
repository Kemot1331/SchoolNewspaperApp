namespace SchoolNewspaperBlazorApp.Data;

public class Permission
{
    public int Id { get; set; }
    public EnumPermissions PermissionsName { get; set; }
    public ICollection<UserPermission> Permissions { get; set; } = new List<UserPermission>();
}