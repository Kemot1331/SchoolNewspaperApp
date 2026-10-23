namespace SchoolNewspaperBlazorApp.Data;

public class Role
{
    public const string MasterName = "Master";
    public const int MaxNameLength = 100;

    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    // Rola "Master" jest chroniona – nadać ją można tylko bezpośrednio w SQL
    public static bool IsMasterName(string? name) =>
        string.Equals(name?.Trim(), MasterName, StringComparison.OrdinalIgnoreCase);
}