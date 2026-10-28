using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using SchoolNewspaperBlazorApp.Data;
using SchoolNewspaperBlazorApp.Interfaces.Repository;
using SchoolNewspaperBlazorApp.Interfaces.Service;

namespace SchoolNewspaperBlazorApp.Service
{
    public class FileService : IFileService
    {
        // Maksymalny rozmiar jednego zdjęcia
        public const long MaxFileSize = 5 * 1024 * 1024;

        private readonly IFileRepository _fileRepository;
        private readonly string uploadPath;
        private readonly IPermissionService _permissionService;
        private readonly ILogger<FileService> _logger;

        public FileService(
            IFileRepository fileRepository,
            IWebHostEnvironment environment,
            IPermissionService permissionService,
            ILogger<FileService> logger)
        {
            _fileRepository = fileRepository;
            _permissionService = permissionService;
            _logger = logger;

            var webRoot = string.IsNullOrEmpty(environment.WebRootPath)
                ? Path.Combine(environment.ContentRootPath, "wwwroot")
                : environment.WebRootPath;

            uploadPath = Path.Combine(webRoot, "Images");
        }

        public async Task<ImageData> ReadImageAsync(IBrowserFile file)
        {
            if (file.Size > MaxFileSize)
            {
                throw new InvalidOperationException($"Plik „{file.Name}” jest za duży (maksymalnie 5 MB).");
            }

            byte[] bytes;
            await using (var stream = file.OpenReadStream(MaxFileSize))
            using (var memory = new MemoryStream())
            {
                await stream.CopyToAsync(memory);
                bytes = memory.ToArray();
            }

            // o typie decyduje zawartość pliku, a nie jego nazwa ani typ podany przez przeglądarkę
            var extension = DetectExtension(bytes)
                ?? throw new InvalidOperationException($"Plik „{file.Name}” nie jest zdjęciem JPG, PNG ani WEBP.");

            return new ImageData(file.Name, extension, bytes, BuildPreview(extension, bytes));
        }

        public async Task<int> UploadImageAsync(ImageData image)
        {
            // zdjęcia dodaje się przy tworzeniu artykułu, ale też przy jego edycji
            var allowed = await _permissionService.HasPermissionAsync(EnumPermissions.AddArticle)
                          || await _permissionService.HasPermissionAsync(EnumPermissions.EditArticle);
            if (!allowed)
            {
                throw new UnauthorizedAccessException();
            }

            if (image.Bytes.Length > MaxFileSize)
            {
                throw new InvalidOperationException("Zdjęcie jest za duże (maksymalnie 5 MB).");
            }

            // rozszerzenie ustalamy ponownie z zawartości – nic od użytkownika nie trafia do nazwy pliku na dysku
            var extension = DetectExtension(image.Bytes)
                ?? throw new InvalidOperationException("Plik nie jest zdjęciem JPG, PNG ani WEBP.");

            Directory.CreateDirectory(uploadPath);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(uploadPath, fileName);
            await File.WriteAllBytesAsync(fullPath, image.Bytes);

            try
            {
                var mediaFile = new MediaFile
                {
                    FileName = fileName,
                    FileType = extension
                };

                await _fileRepository.AddFileAsync(mediaFile);
                return mediaFile.Id;
            }
            catch
            {
                // nie zostawiamy na dysku pliku, którego nie ma w bazie
                TryDelete(fullPath);
                throw;
            }
        }

        public async Task<List<int>> UploadImagesAsync(IEnumerable<ImageData> images)
        {
            var list = images.ToList();
            var ids = new List<int>();

            foreach (var image in list)
            {
                ids.Add(await UploadImageAsync(image));
            }

            return ids;
        }

        public async Task<string> GetFileNameByIdAsync(int id)
        {
            var file = await _fileRepository.GetFileByIdAsync(id);
            return file.FileName;
        }

        public async Task<string?> TryGetFileNameByIdAsync(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            try
            {
                var file = await _fileRepository.GetFileByIdAsync(id);
                return string.IsNullOrWhiteSpace(file.FileName) ? null : file.FileName;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Nie znaleziono pliku o Id {FileId}", id);
                return null;
            }
        }

        // Rozpoznaje JPG / PNG / WEBP po pierwszych bajtach pliku
        private static string? DetectExtension(ReadOnlySpan<byte> b)
        {
            if (b.Length >= 3 && b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF)
            {
                return ".jpg";
            }

            if (b.Length >= 8
                && b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47
                && b[4] == 0x0D && b[5] == 0x0A && b[6] == 0x1A && b[7] == 0x0A)
            {
                return ".png";
            }

            if (b.Length >= 12
                && b[0] == 'R' && b[1] == 'I' && b[2] == 'F' && b[3] == 'F'
                && b[8] == 'W' && b[9] == 'E' && b[10] == 'B' && b[11] == 'P')
            {
                return ".webp";
            }

            return null;
        }

        private static string BuildPreview(string extension, byte[] bytes)
        {
            var mime = extension switch
            {
                ".png" => "image/png",
                ".webp" => "image/webp",
                _ => "image/jpeg"
            };

            return $"data:{mime};base64,{Convert.ToBase64String(bytes)}";
        }

        private void TryDelete(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Nie udało się usunąć pliku {Path}", path);
            }
        }
    }
}
