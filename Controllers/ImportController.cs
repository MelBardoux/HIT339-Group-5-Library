using LibrarySystem.Data;
using LibrarySystem.Models;
using LibrarySystem.Services;
using LibrarySystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Controllers
{
    [Authorize(Roles = "Admin,Manager,Reception")]
    public class ImportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CsvImportService _importService;
        private readonly ILogger<ImportController> _logger;
        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

        public ImportController(ApplicationDbContext context, CsvImportService importService, ILogger<ImportController> logger)
        {
            _context = context;
            _importService = importService;
            _logger = logger;
        }

        // GET: Import/Index
        public async Task<IActionResult> Index()
        {
            var branches = await _context.Branches.ToListAsync();
            var viewModel = new CsvImportViewModel
            {
                Branches = branches,
                ItemTypes = new List<string> { "Book", "Music" }
            };
            return View(viewModel);
        }

        // POST: Import/Preview
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Preview(CsvImportViewModel viewModel)
        {
            if (viewModel.File == null || viewModel.File.Length == 0)
            {
                ModelState.AddModelError("File", "Please select a file to upload.");
                viewModel.Branches = await _context.Branches.ToListAsync();
                viewModel.ItemTypes = new List<string> { "Book", "Music" };
                return View("Index", viewModel);
            }

            if (viewModel.File.Length > MaxFileSize)
            {
                ModelState.AddModelError("File", $"File size exceeds maximum allowed size of 5MB.");
                viewModel.Branches = await _context.Branches.ToListAsync();
                viewModel.ItemTypes = new List<string> { "Book", "Music" };
                return View("Index", viewModel);
            }

            if (!Path.GetExtension(viewModel.File.FileName).Equals(".csv", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("File", "Please upload a CSV file.");
                viewModel.Branches = await _context.Branches.ToListAsync();
                viewModel.ItemTypes = new List<string> { "Book", "Music" };
                return View("Index", viewModel);
            }

            try
            {
                using (var stream = new StreamReader(viewModel.File.OpenReadStream()))
                {
                    var csvContent = await stream.ReadToEndAsync();
                    var branch = await _context.Branches.FirstOrDefaultAsync(b => b.Id == viewModel.BranchId);

                    if (branch == null)
                    {
                        ModelState.AddModelError("BranchId", "Selected branch not found.");
                        viewModel.Branches = await _context.Branches.ToListAsync();
                        viewModel.ItemTypes = new List<string> { "Book", "Music" };
                        return View("Index", viewModel);
                    }

                    var previewResult = await _importService.PreviewCsvAsync(csvContent, viewModel.ItemType, branch);

                    viewModel.PreviewItems = previewResult.PreviewItems;
                    viewModel.Errors = previewResult.Errors;
                    viewModel.CsvContent = csvContent;
                    viewModel.TotalRows = previewResult.TotalRows;
                    viewModel.ValidRows = previewResult.ValidRows;
                    viewModel.Branches = await _context.Branches.ToListAsync();
                    viewModel.ItemTypes = new List<string> { "Book", "Music" };

                    if (!previewResult.Errors.Any())
                    {
                        viewModel.Success = true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error previewing CSV file");
                ModelState.AddModelError("File", $"Error processing file: {ex.Message}");
                viewModel.Branches = await _context.Branches.ToListAsync();
                viewModel.ItemTypes = new List<string> { "Book", "Music" };
                return View("Index", viewModel);
            }

            return View("Preview", viewModel);
        }

        // POST: Import/Process
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Process(CsvImportViewModel viewModel)
        {
            if (string.IsNullOrEmpty(viewModel.CsvContent))
            {
                return Json(new { success = false, message = "No CSV data provided." });
            }

            try
            {
                var branch = await _context.Branches.FirstOrDefaultAsync(b => b.Id == viewModel.BranchId);
                if (branch == null)
                {
                    return Json(new { success = false, message = "Branch not found." });
                }

                var result = await _importService.ImportCsvAsync(viewModel.CsvContent, viewModel.ItemType, branch);

                if (result.Success)
                {
                    return Json(new
                    {
                        success = true,
                        message = $"Successfully imported {result.ImportedCount} items.",
                        importedCount = result.ImportedCount,
                        errorCount = result.Errors.Count
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Import failed",
                        errors = result.Errors
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing CSV import");
                return Json(new { success = false, message = $"Error during import: {ex.Message}" });
            }
        }
    }
}
