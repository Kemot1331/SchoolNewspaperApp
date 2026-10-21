using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;
using SchoolNewspaperBlazorApp.Data;
using SchoolNewspaperBlazorApp.Interfaces.Repository;
using SchoolNewspaperBlazorApp.Interfaces.Service;

namespace SchoolNewspaperBlazorApp.Service
{
    public class FileService : IFileService
    {
        private readonly IFileRepository _fileRepository;
        private readonly string uploadPath;
        private readonly IPermissionService _permissionService;

        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        
        public FileService(
            IFileRepository fileRepository,
            IWebHostEnvironment environment,
            IPermissionService permissionService)
        {
            _fileRepository = fileRepository;
            _permissionService = permissionService;

            uploadPath = Path.Combine(
                environment.ContentRootPath,
                "wwwroot",
                "Images");
        }

        public async Task<string> GetPreviewAsync(IBrowserFile file)
        {
            using var stream = file.OpenReadStream(5 * 1024 * 1024);
            using var memoryStream = new MemoryStream();

            await stream.CopyToAsync(memoryStream);

            byte[] bytes = memoryStream.ToArray();

            return $"data:{file.ContentType};base64,{Convert.ToBase64String(bytes)}";
        }

        public async Task<int> UploadImage(IBrowserFile file)
        {
            bool hasPermission =  await _permissionService.HasPermissionAsync(EnumPermissions.AddArticle);
            bool isMaster = await _permissionService.HasPermissionAsync(EnumPermissions.Master);
            if (!hasPermission && !isMaster)
            {
                throw new Exception("You do not have permission to upload images.");
            }
            Directory.CreateDirectory(uploadPath);

            string extension = Path.GetExtension(file.Name);
            string fileName = $"{Guid.NewGuid()}{extension}";
            string fullPath = Path.Combine(uploadPath, fileName);

            await using (var stream = file.OpenReadStream(5 * 1024 * 1024))
            await using (var fileStream = new FileStream(
                fullPath,
                FileMode.CreateNew,
                FileAccess.Write))
            {
                await stream.CopyToAsync(fileStream);
            }

            // Uwaga: sposób generowania ID powinien być zgodny
            // z konfiguracją klucza głównego w bazie.
            var id = await _fileRepository.GetLastFileId();

            var mediaFile = new MediaFile
            {
                FileName = fileName,
                FileType = extension
            };

            await _fileRepository.AddFileAsync(mediaFile);

            return mediaFile.Id;
        }

        public async Task<string> GetFileNameByIdAsync(int id)
        {
            var file = await _fileRepository.GetFileByIdAsync(id);
            return file.FileName;
        }

        public async Task<List<int>> UploadImagesAsync(IEnumerable<IBrowserFile> files)
        {
            var list = files.ToList();

            foreach (var file in list)
            {
                var extension = Path.GetExtension(file.Name).ToLowerInvariant();
                if (!AllowedExtensions.Contains(extension))
                {
                    throw new InvalidOperationException("Invalid image extension.");
                }
            }
            var ids = new List<int>();
            foreach (var file in list)
            {
                ids.Add(await UploadImage(file));
            }

            return ids;
        }
    }
}