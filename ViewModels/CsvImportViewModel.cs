using LibrarySystem.Models;
using LibrarySystem.Services;
using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.ViewModels
{
    public class CsvImportViewModel
    {
        [Required(ErrorMessage = "Please select a file to upload")]
        public IFormFile File { get; set; }

        [Required(ErrorMessage = "Please select an item type")]
        public string ItemType { get; set; } = "Book";

        [Required(ErrorMessage = "Please select a branch")]
        public int BranchId { get; set; }

        public List<Branch> Branches { get; set; } = new();
        public List<string> ItemTypes { get; set; } = new();

        // For preview and results
        public List<CsvItemPreview> PreviewItems { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public string CsvContent { get; set; }
        public int TotalRows { get; set; }
        public int ValidRows { get; set; }
        public bool Success { get; set; }

        public string GetFileSizeKB(IFormFile file)
        {
            if (file == null) return "0";
            return (file.Length / 1024).ToString();
        }
    }
}
