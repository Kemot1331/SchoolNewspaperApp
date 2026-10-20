namespace SchoolNewspaperBlazorApp.Service;

public record Toast(int Id, string Message, string Type);

public class ToastService
{
    private int _nextId;
    public event Action? OnChange;
    public List<Toast> Toasts { get; } = new();

    public void Success(string text) => Show(text, "success");
    public void Error(string text) => Show(text, "error");
    
    private void Show(string text, string type)
    {
        var toast = new Toast(++_nextId, text, type);
        Toasts.Add(toast);
        OnChange?.Invoke();
        _ = DismissLaterAsync(toast.Id);
    }
    public void Remove(int id)
    {
        Toasts.RemoveAll(t => t.Id == id);
        OnChange?.Invoke();
    }

    private async Task DismissLaterAsync(int id)
    {
        await Task.Delay(4000);
        Remove(id);
    }
}