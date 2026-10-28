using Microsoft.AspNetCore.Components.Forms;
using SchoolNewspaperBlazorApp.Data;

namespace SchoolNewspaperBlazorApp.Interfaces.Service
{
    public interface IFileService
    {
        // Wczytuje wybrany plik do pamięci i sprawdza, czy to naprawdę JPG/PNG/WEBP (maks. 5 MB).
        // Rzuca InvalidOperationException z czytelnym komunikatem, gdy plik się nie nadaje.
        Task<ImageData> ReadImageAsync(IBrowserFile file);

        // Zapisuje zdjęcie na dysk i w bazie, zwraca jego Id.
        Task<int> UploadImageAsync(ImageData image);
        Task<List<int>> UploadImagesAsync(IEnumerable<ImageData> images);

        Task<string> GetFileNameByIdAsync(int id);

        // To samo co wyżej, ale zwraca null zamiast rzucać wyjątek, gdy pliku nie ma w bazie.
        Task<string?> TryGetFileNameByIdAsync(int id);
    }
}
