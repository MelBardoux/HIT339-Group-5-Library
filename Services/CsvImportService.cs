using LibrarySystem.Data;
using LibrarySystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Services
{
    public class CsvImportService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CsvImportService> _logger;

        public CsvImportService(ApplicationDbContext context, ILogger<CsvImportService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PreviewResult> PreviewCsvAsync(string csvContent, string itemType, Branch branch)
        {
            var result = new PreviewResult
            {
                PreviewItems = new List<CsvItemPreview>(),
                Errors = new List<string>()
            };

            try
            {
                var lines = csvContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                if (lines.Length < 2)
                {
                    result.Errors.Add("CSV file must contain at least a header row and one data row.");
                    return result;
                }

                result.TotalRows = lines.Length - 1; // Exclude header

                var headers = ParseCsvLine(lines[0]);
                ValidateCsvHeaders(headers, itemType, result);

                if (result.Errors.Any())
                {
                    return result;
                }

                var validRows = 0;
                var previewCount = 0;

                for (int i = 1; i < lines.Length; i++)
                {
                    var values = ParseCsvLine(lines[i]);
                    var preview = new CsvItemPreview
                    {
                        RowNumber = i + 1,
                        IsValid = true,
                        Errors = new List<string>()
                    };

                    if (itemType.Equals("Book", StringComparison.OrdinalIgnoreCase))
                    {
                        await ValidateBookRow(headers, values, preview, branch);
                    }
                    else if (itemType.Equals("Music", StringComparison.OrdinalIgnoreCase))
                    {
                        await ValidateMusicRow(headers, values, preview, branch);
                    }

                    if (preview.IsValid)
                    {
                        validRows++;
                        if (previewCount < 5)
                        {
                            result.PreviewItems.Add(preview);
                            previewCount++;
                        }
                    }
                    else
                    {
                        result.Errors.AddRange(preview.Errors.Select(e => $"Row {i + 1}: {e}"));
                    }
                }

                result.ValidRows = validRows;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error previewing CSV");
                result.Errors.Add($"Error parsing CSV: {ex.Message}");
            }

            return result;
        }

        public async Task<ImportResult> ImportCsvAsync(string csvContent, string itemType, Branch branch)
        {
            var result = new ImportResult
            {
                Success = true,
                ImportedCount = 0,
                Errors = new List<string>()
            };

            try
            {
                var lines = csvContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                if (lines.Length < 2)
                {
                    result.Success = false;
                    result.Errors.Add("CSV file must contain at least a header row and one data row.");
                    return result;
                }

                var headers = ParseCsvLine(lines[0]);
                ValidateCsvHeaders(headers, itemType, new PreviewResult { Errors = result.Errors });

                if (result.Errors.Any())
                {
                    result.Success = false;
                    return result;
                }

                for (int i = 1; i < lines.Length; i++)
                {
                    var values = ParseCsvLine(lines[i]);

                    try
                    {
                        if (itemType.Equals("Book", StringComparison.OrdinalIgnoreCase))
                        {
                            await ImportBookRow(headers, values, branch);
                        }
                        else if (itemType.Equals("Music", StringComparison.OrdinalIgnoreCase))
                        {
                            await ImportMusicRow(headers, values, branch);
                        }

                        result.ImportedCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error importing row {i + 1}");
                        result.Errors.Add($"Row {i + 1}: {ex.Message}");
                    }
                }

                if (result.ImportedCount > 0)
                {
                    await _context.SaveChangesAsync();
                }

                result.Success = result.Errors.Count == 0 || result.ImportedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing CSV");
                result.Success = false;
                result.Errors.Add($"Import error: {ex.Message}");
            }

            return result;
        }

        private List<string> ParseCsvLine(string line)
        {
            var result = new List<string>();
            var current = new System.Text.StringBuilder();
            var inQuotes = false;

            foreach (var c in line)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current.ToString().Trim());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            result.Add(current.ToString().Trim());
            return result;
        }

        private void ValidateCsvHeaders(List<string> headers, string itemType, PreviewResult result)
        {
            var requiredHeaders = new List<string> { "Name", "Description", "LibraryCode" };

            if (itemType.Equals("Book", StringComparison.OrdinalIgnoreCase))
            {
                requiredHeaders.AddRange(new[] { "AuthorName", "PublicationYear" });
            }
            else if (itemType.Equals("Music", StringComparison.OrdinalIgnoreCase))
            {
                requiredHeaders.AddRange(new[] { "AlbumTitle", "ArtistName", "ReleaseYear" });
            }

            foreach (var required in requiredHeaders)
            {
                if (!headers.Any(h => h.Equals(required, StringComparison.OrdinalIgnoreCase)))
                {
                    result.Errors.Add($"Missing required column: {required}");
                }
            }
        }

        private async Task ValidateBookRow(List<string> headers, List<string> values, CsvItemPreview preview, Branch branch)
        {
            if (values.Count < headers.Count)
            {
                preview.IsValid = false;
                preview.Errors.Add("Row has fewer columns than header.");
                return;
            }

            var nameIdx = FindColumnIndex(headers, "Name");
            var descIdx = FindColumnIndex(headers, "Description");
            var codeIdx = FindColumnIndex(headers, "LibraryCode");
            var authorIdx = FindColumnIndex(headers, "AuthorName");
            var yearIdx = FindColumnIndex(headers, "PublicationYear");

            var name = GetValue(values, nameIdx);
            var code = GetValue(values, codeIdx);
            var authorName = GetValue(values, authorIdx);

            if (string.IsNullOrWhiteSpace(name))
            {
                preview.IsValid = false;
                preview.Errors.Add("Name is required.");
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                preview.IsValid = false;
                preview.Errors.Add("LibraryCode is required.");
            }
            else if (!System.Text.RegularExpressions.Regex.IsMatch(code, @"^[A-Z]{3}-[0-9]{4}$"))
            {
                preview.IsValid = false;
                preview.Errors.Add("LibraryCode must be in format XXX-NNNN (e.g., BOO-0001).");
            }
            else
            {
                var exists = await _context.Items.AnyAsync(i => i.LibraryCode == code);
                if (exists)
                {
                    preview.IsValid = false;
                    preview.Errors.Add($"LibraryCode '{code}' already exists.");
                }
            }

            if (string.IsNullOrWhiteSpace(authorName))
            {
                preview.IsValid = false;
                preview.Errors.Add("AuthorName is required.");
            }

            if (preview.IsValid)
            {
                preview.ItemName = name;
                preview.AuthorArtistName = authorName;
                preview.LibraryCode = code;
            }
        }

        private async Task ValidateMusicRow(List<string> headers, List<string> values, CsvItemPreview preview, Branch branch)
        {
            if (values.Count < headers.Count)
            {
                preview.IsValid = false;
                preview.Errors.Add("Row has fewer columns than header.");
                return;
            }

            var nameIdx = FindColumnIndex(headers, "Name");
            var descIdx = FindColumnIndex(headers, "Description");
            var codeIdx = FindColumnIndex(headers, "LibraryCode");
            var albumIdx = FindColumnIndex(headers, "AlbumTitle");
            var artistIdx = FindColumnIndex(headers, "ArtistName");
            var yearIdx = FindColumnIndex(headers, "ReleaseYear");

            var name = GetValue(values, nameIdx);
            var code = GetValue(values, codeIdx);
            var album = GetValue(values, albumIdx);
            var artist = GetValue(values, artistIdx);

            if (string.IsNullOrWhiteSpace(name))
            {
                preview.IsValid = false;
                preview.Errors.Add("Name is required.");
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                preview.IsValid = false;
                preview.Errors.Add("LibraryCode is required.");
            }
            else if (!System.Text.RegularExpressions.Regex.IsMatch(code, @"^[A-Z]{3}-[0-9]{4}$"))
            {
                preview.IsValid = false;
                preview.Errors.Add("LibraryCode must be in format XXX-NNNN (e.g., MUS-0001).");
            }
            else
            {
                var exists = await _context.Items.AnyAsync(i => i.LibraryCode == code);
                if (exists)
                {
                    preview.IsValid = false;
                    preview.Errors.Add($"LibraryCode '{code}' already exists.");
                }
            }

            if (string.IsNullOrWhiteSpace(album))
            {
                preview.IsValid = false;
                preview.Errors.Add("AlbumTitle is required.");
            }

            if (string.IsNullOrWhiteSpace(artist))
            {
                preview.IsValid = false;
                preview.Errors.Add("ArtistName is required.");
            }

            if (preview.IsValid)
            {
                preview.ItemName = name;
                preview.AuthorArtistName = artist;
                preview.LibraryCode = code;
            }
        }

        private async Task ImportBookRow(List<string> headers, List<string> values, Branch branch)
        {
            var nameIdx = FindColumnIndex(headers, "Name");
            var descIdx = FindColumnIndex(headers, "Description");
            var codeIdx = FindColumnIndex(headers, "LibraryCode");
            var authorIdx = FindColumnIndex(headers, "AuthorName");
            var yearIdx = FindColumnIndex(headers, "PublicationYear");
            var genreIdx = FindColumnIndex(headers, "Genre");

            var name = GetValue(values, nameIdx);
            var description = GetValue(values, descIdx);
            var code = GetValue(values, codeIdx);
            var authorName = GetValue(values, authorIdx);
            var yearStr = GetValue(values, yearIdx);
            var genreStr = GetValue(values, genreIdx);

            // Find or create author
            var author = await _context.Authors.FirstOrDefaultAsync(a => a.Name == authorName);
            if (author == null)
            {
                author = new Author { Name = authorName };
                _context.Authors.Add(author);
                await _context.SaveChangesAsync();
            }

            // Create book
            var book = new Book
            {
                Name = name,
                Description = description,
                LibraryCode = code,
                Status = ItemStatus.Available,
                DateAdded = DateOnly.FromDateTime(DateTime.Now),
                BranchId = branch.Id,
                AuthorId = author.Id,
                PublicationYearUnknown = string.IsNullOrWhiteSpace(yearStr)
            };

            if (!string.IsNullOrWhiteSpace(yearStr) && int.TryParse(yearStr, out var year))
            {
                book.PublicationYear = year;
            }

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            // Add genres if specified
            if (!string.IsNullOrWhiteSpace(genreStr) && genreIdx >= 0)
            {
                var genres = genreStr.Split(';', StringSplitOptions.RemoveEmptyEntries);
                foreach (var g in genres)
                {
                    var genre = await _context.BookGenres.FirstOrDefaultAsync(bg => bg.Name == g.Trim());
                    if (genre != null)
                    {
                        book.Genres.Add(genre);
                    }
                }
                await _context.SaveChangesAsync();
            }
        }

        private async Task ImportMusicRow(List<string> headers, List<string> values, Branch branch)
        {
            var nameIdx = FindColumnIndex(headers, "Name");
            var descIdx = FindColumnIndex(headers, "Description");
            var codeIdx = FindColumnIndex(headers, "LibraryCode");
            var albumIdx = FindColumnIndex(headers, "AlbumTitle");
            var artistIdx = FindColumnIndex(headers, "ArtistName");
            var yearIdx = FindColumnIndex(headers, "ReleaseYear");
            var genreIdx = FindColumnIndex(headers, "Genre");
            var formatIdx = FindColumnIndex(headers, "Format");

            var name = GetValue(values, nameIdx);
            var description = GetValue(values, descIdx);
            var code = GetValue(values, codeIdx);
            var album = GetValue(values, albumIdx);
            var artistName = GetValue(values, artistIdx);
            var yearStr = GetValue(values, yearIdx);
            var genreStr = GetValue(values, genreIdx);
            var formatStr = GetValue(values, formatIdx);

            // Find or create artist
            var artist = await _context.Artists.FirstOrDefaultAsync(a => a.Name == artistName);
            if (artist == null)
            {
                artist = new Artist { Name = artistName };
                _context.Artists.Add(artist);
                await _context.SaveChangesAsync();
            }

            // Create music
            var music = new Music
            {
                Name = name,
                Description = description,
                LibraryCode = code,
                AlbumTitle = album,
                Status = ItemStatus.Available,
                DateAdded = DateOnly.FromDateTime(DateTime.Now),
                BranchId = branch.Id,
                ReleaseYearUnknown = string.IsNullOrWhiteSpace(yearStr)
            };

            if (!string.IsNullOrWhiteSpace(yearStr) && int.TryParse(yearStr, out var year))
            {
                music.ReleaseYear = year;
            }

            music.Artists.Add(artist);
            _context.Music.Add(music);
            await _context.SaveChangesAsync();

            // Add genres if specified
            if (!string.IsNullOrWhiteSpace(genreStr))
            {
                var genres = genreStr.Split(';', StringSplitOptions.RemoveEmptyEntries);
                foreach (var g in genres)
                {
                    var genre = await _context.MusicGenres.FirstOrDefaultAsync(mg => mg.Name == g.Trim());
                    if (genre != null)
                    {
                        music.Genres.Add(genre);
                    }
                }
                await _context.SaveChangesAsync();
            }

            // Add formats if specified
            if (!string.IsNullOrWhiteSpace(formatStr))
            {
                var formats = formatStr.Split(';', StringSplitOptions.RemoveEmptyEntries);
                foreach (var f in formats)
                {
                    var format = await _context.MusicFormats.FirstOrDefaultAsync(mf => mf.Name == f.Trim());
                    if (format != null)
                    {
                        music.Formats.Add(format);
                    }
                }
                await _context.SaveChangesAsync();
            }
        }

        private int FindColumnIndex(List<string> headers, string columnName)
        {
            return headers.FindIndex(h => h.Equals(columnName, StringComparison.OrdinalIgnoreCase));
        }

        private string GetValue(List<string> values, int index)
        {
            return index >= 0 && index < values.Count ? values[index] : string.Empty;
        }
    }

    public class PreviewResult
    {
        public List<CsvItemPreview> PreviewItems { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public int TotalRows { get; set; }
        public int ValidRows { get; set; }
    }

    public class ImportResult
    {
        public bool Success { get; set; }
        public int ImportedCount { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class CsvItemPreview
    {
        public int RowNumber { get; set; }
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public string ItemName { get; set; }
        public string AuthorArtistName { get; set; }
        public string LibraryCode { get; set; }
    }
}
