using LibrarySystem.Data;
using LibrarySystem.Models;

namespace LibrarySystem.Services
{
    /// <summary>
    /// Enriches CSV import data with external metadata
    /// </summary>
    public interface IMetadataEnrichmentService
    {
        Task<CsvItemEnrichmentResult> EnrichBookWithExternalMetadataAsync(
            string authorName, string title, string? isbn = null);

        Task<CsvItemEnrichmentResult> EnrichMusicWithExternalMetadataAsync(
            string artistName, string albumTitle, string? isrc = null);

        Task<List<CsvItemEnrichmentResult>> EnrichMultipleItemsAsync(
            List<CsvItemEnrichmentRequest> requests, string itemType);
    }

    public class CsvItemEnrichmentRequest
    {
        public int RowNumber { get; set; }
        public string AuthorOrArtist { get; set; } = string.Empty;
        public string TitleOrAlbum { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public string ISRC { get; set; } = string.Empty;
    }

    public class CsvItemEnrichmentResult
    {
        public int RowNumber { get; set; }
        public bool IsEnriched { get; set; }
        public string EnrichmentSource { get; set; } = string.Empty;
        public string OriginalTitle { get; set; } = string.Empty;
        public string EnrichedTitle { get; set; } = string.Empty;
        public string EnrichedDescription { get; set; } = string.Empty;
        public string EnrichedAuthorOrArtist { get; set; } = string.Empty;
        public int? EnrichedYear { get; set; }
        public List<string> EnrichedCategories { get; set; } = new();
        public string ISBN { get; set; } = string.Empty;
        public decimal? Rating { get; set; }
        public string CoverImageUrl { get; set; } = string.Empty;
        public string ConfidenceScore { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class MetadataEnrichmentService : IMetadataEnrichmentService
    {
        private readonly IExternalMetadataProvider _metadataProvider;
        private readonly ILogger<MetadataEnrichmentService> _logger;

        public MetadataEnrichmentService(
            IExternalMetadataProvider metadataProvider,
            ILogger<MetadataEnrichmentService> logger)
        {
            _metadataProvider = metadataProvider;
            _logger = logger;
        }

        public async Task<CsvItemEnrichmentResult> EnrichBookWithExternalMetadataAsync(
            string authorName, string title, string? isbn = null)
        {
            var result = new CsvItemEnrichmentResult
            {
                OriginalTitle = title,
                EnrichedAuthorOrArtist = authorName
            };

            try
            {
                ExternalBookMetadata? metadata = null;

                // Try ISBN lookup first if available
                if (!string.IsNullOrWhiteSpace(isbn))
                {
                    _logger.LogInformation($"Attempting ISBN lookup for: {isbn}");
                    metadata = await _metadataProvider.GetBookMetadataByIsbnAsync(isbn);
                    if (metadata != null)
                    {
                        result.ISBN = metadata.ISBN13 ?? metadata.ISBN;
                    }
                }

                // Fall back to author+title lookup
                if (metadata == null && !string.IsNullOrWhiteSpace(authorName) && !string.IsNullOrWhiteSpace(title))
                {
                    _logger.LogInformation($"Attempting author+title lookup for: {authorName} - {title}");
                    metadata = await _metadataProvider.GetBookMetadataByAuthorAndTitleAsync(authorName, title);
                }

                if (metadata != null)
                {
                    result.IsEnriched = true;
                    result.EnrichmentSource = "Open Library & OpenShelves";
                    result.EnrichedTitle = metadata.Title;
                    result.EnrichedDescription = metadata.Description;
                    result.EnrichedAuthorOrArtist = metadata.AuthorName;
                    result.EnrichedYear = metadata.PublicationYear;
                    result.EnrichedCategories = metadata.Categories;
                    result.Rating = metadata.AverageRating;
                    result.CoverImageUrl = metadata.CoverImageUrl;
                    result.ConfidenceScore = metadata.ConfidenceScore;
                    result.ISBN = metadata.ISBN13 ?? metadata.ISBN;

                    _logger.LogInformation($"Successfully enriched book: {title} with confidence: {metadata.ConfidenceScore}");
                }
                else
                {
                    result.IsEnriched = false;
                    result.ErrorMessage = "No matching metadata found in external sources";
                    _logger.LogWarning($"No metadata found for book: {title} by {authorName}");
                }
            }
            catch (Exception ex)
            {
                result.IsEnriched = false;
                result.ErrorMessage = $"Error during enrichment: {ex.Message}";
                _logger.LogError(ex, $"Error enriching book metadata for {title}");
            }

            return result;
        }

        public async Task<CsvItemEnrichmentResult> EnrichMusicWithExternalMetadataAsync(
            string artistName, string albumTitle, string? isrc = null)
        {
            var result = new CsvItemEnrichmentResult
            {
                OriginalTitle = albumTitle,
                EnrichedAuthorOrArtist = artistName
            };

            try
            {
                ExternalMusicMetadata? metadata = null;

                // Try ISRC lookup first if available
                if (!string.IsNullOrWhiteSpace(isrc))
                {
                    _logger.LogInformation($"Attempting ISRC lookup for: {isrc}");
                    metadata = await _metadataProvider.GetMusicMetadataByIsrcAsync(isrc);
                }

                // Fall back to artist+album lookup
                if (metadata == null && !string.IsNullOrWhiteSpace(artistName) && !string.IsNullOrWhiteSpace(albumTitle))
                {
                    _logger.LogInformation($"Attempting artist+album lookup for: {artistName} - {albumTitle}");
                    metadata = await _metadataProvider.GetMusicMetadataByArtistAndAlbumAsync(artistName, albumTitle);
                }

                if (metadata != null)
                {
                    result.IsEnriched = true;
                    result.EnrichmentSource = "Spotify & MusicBrainz";
                    result.EnrichedTitle = metadata.AlbumTitle;
                    result.EnrichedDescription = metadata.Description;
                    result.EnrichedAuthorOrArtist = metadata.ArtistName;
                    result.EnrichedYear = metadata.ReleaseYear;
                    result.EnrichedCategories = metadata.Genres;
                    result.Rating = metadata.AverageRating;
                    result.CoverImageUrl = metadata.CoverImageUrl;
                    result.ConfidenceScore = metadata.ConfidenceScore;

                    _logger.LogInformation($"Successfully enriched music: {albumTitle} - {artistName} with confidence: {metadata.ConfidenceScore}");
                }
                else
                {
                    result.IsEnriched = false;
                    result.ErrorMessage = "No matching metadata found in external sources";
                    _logger.LogWarning($"No metadata found for music: {albumTitle} by {artistName}");
                }
            }
            catch (Exception ex)
            {
                result.IsEnriched = false;
                result.ErrorMessage = $"Error during enrichment: {ex.Message}";
                _logger.LogError(ex, $"Error enriching music metadata for {albumTitle}");
            }

            return result;
        }

        public async Task<List<CsvItemEnrichmentResult>> EnrichMultipleItemsAsync(
            List<CsvItemEnrichmentRequest> requests, string itemType)
        {
            var results = new List<CsvItemEnrichmentResult>();

            _logger.LogInformation($"Starting enrichment of {requests.Count} {itemType} items");

            foreach (var request in requests)
            {
                CsvItemEnrichmentResult result;

                if (itemType.Equals("Book", StringComparison.OrdinalIgnoreCase))
                {
                    result = await EnrichBookWithExternalMetadataAsync(
                        request.AuthorOrArtist,
                        request.TitleOrAlbum,
                        request.ISBN);
                }
                else if (itemType.Equals("Music", StringComparison.OrdinalIgnoreCase))
                {
                    result = await EnrichMusicWithExternalMetadataAsync(
                        request.AuthorOrArtist,
                        request.TitleOrAlbum,
                        request.ISRC);
                }
                else
                {
                    result = new CsvItemEnrichmentResult
                    {
                        RowNumber = request.RowNumber,
                        IsEnriched = false,
                        ErrorMessage = $"Unknown item type: {itemType}"
                    };
                }

                result.RowNumber = request.RowNumber;
                results.Add(result);
            }

            var enrichedCount = results.Count(r => r.IsEnriched);
            _logger.LogInformation($"Enrichment completed: {enrichedCount}/{requests.Count} items successfully enriched");

            return results;
        }
    }
}
