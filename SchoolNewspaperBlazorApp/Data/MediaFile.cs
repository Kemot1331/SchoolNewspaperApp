using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolNewspaperBlazorApp.Data
{
    public class MediaFile
    {
        public int Id { get; set; }
        public required string FileType { get; set; }
        public required string FileName { get; set; }

    }
}
