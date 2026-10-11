using Microsoft.AspNetCore.Components.Forms;
using SchoolNewspaperBlazorApp.Data;

namespace SchoolNewspaperBlazorApp.Interfaces.Repository
{
    public interface IFileRepository
    {
        Task AddFileAsync(MediaFile mediaFile);
        Task<int> GetLastFileId();
    }
}
