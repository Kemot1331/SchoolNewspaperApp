namespace SchoolNewspaperBlazorApp.Data;

public class UserPermission
{
    public int UserId { get; set; }
    public int PermissionId { get; set; }
    public string PermissionName { get; set; }
    public User User { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}