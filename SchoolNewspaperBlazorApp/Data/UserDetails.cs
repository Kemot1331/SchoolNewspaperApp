namespace SchoolNewspaperBlazorApp.Data;

// Dane konta potrzebne w formularzu edycji (bez hasła)
public record UserDetails(int Id, string Login, string Email, string FirstName, string LastName);
