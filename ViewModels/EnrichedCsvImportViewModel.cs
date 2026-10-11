using LibrarySystem.Services;

namespace LibrarySystem.ViewModels
{
    public class EnrichedCsvImportViewModel : CsvImportViewModel
    {
        /// <summary>
        /// List of items that have been enriched with external metadata
        /// </summary>
        public List<EnrichedItemPreview> EnrichedItems { get; set; } = new();

        /// <summary>
        /// Count of successfully enriched items
        /// </summary>
        public int EnrichedCount { get; set; }

        /// <summary>
        /// Count of items that could not be enriched
        /// </summary>
        public int NotEnrichedCount { get; set; }

        /// <summary>
        /// Overall enrichment metadata
        /// </summary>
        public EnrichmentMetadata EnrichmentMetadata { get; set; } = new();
    }

    public class EnrichedItemPreview
    {
        public int RowNumber { get; set; }
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();

        // Original CSV data
        public string OriginalItemName { get; set; }
        public string OriginalAuthorOrArtist { get; set; }
        public string OriginalDescription { get; set; }
        public int? OriginalYear { get; set; }

        // Enriched data
        public bool IsEnriched { get; set; }
        public string EnrichmentSource { get; set; }
        public string EnrichedItemName { get; set; }
        public string EnrichedAuthorOrArtist { get; set; }
        public string EnrichedDescription { get; set; }
        public int? EnrichedYear { get; set; }
        public List<string> EnrichedCategories { get; set; } = new();
        public string EnrichedLibraryCode { get; set; }
        public decimal? EnrichedRating { get; set; }
        public string EnrichedCoverImageUrl { get; set; }
        public string ConfidenceScore { get; set; }
        public string EnrichmentErrorMessage { get; set; }

        // Show changes
        public bool HasNameChange => OriginalItemName != EnrichedItemName;
        public bool HasAuthorChange => OriginalAuthorOrArtist != EnrichedAuthorOrArtist;
        public bool HasDescriptionChange => OriginalDescription != EnrichedDescription;
        public bool HasYearChange => OriginalYear != EnrichedYear;
    }

    public class EnrichmentMetadata
    {
        public DateTime ProcessedAt { get; set; } = DateTime.Now;
        public TimeSpan TotalEnrichmentTime { get; set; }
        public string PrimaryMetadataSources { get; set; } = "Open Library, Spotify, MusicBrainz";
        public int AverageTimePerItem { get; set; }
        public string ConfidenceLevel { get; set; } = "High";
        public List<string> ConfidenceLevelBreakdown { get; set; } = new();

        public string GetConfidenceSummary()
        {
            return $"Enrichment completed in {TotalEnrichmentTime.TotalSeconds:F2}s";
        }
    }

    public class MetadataComparisonViewModel
    {
        public string ItemType { get; set; }
        public int RowNumber { get; set; }

        // Original CSV data
        public string OriginalTitle { get; set; }
        public string OriginalAuthor { get; set; }
        public string OriginalDescription { get; set; }

        // External metadata
        public string ExternalTitle { get; set; }
        public string ExternalAuthor { get; set; }
        public string ExternalDescription { get; set; }
        public List<string> ExternalCategories { get; set; } = new();
        public decimal? ExternalRating { get; set; }
        public string ExternalCoverUrl { get; set; }
        public int? ExternalYear { get; set; }
        public string ConfidenceScore { get; set; }
        public string Source { get; set; }
    }

    public class BatchEnrichmentStatusViewModel
    {
        public int TotalItems { get; set; }
        public int ProcessedItems { get; set; }
        public int EnrichedItems { get; set; }
        public int FailedItems { get; set; }
        public int SkippedItems { get; set; }
        public double PercentComplete { get; set; }
        public TimeSpan ElapsedTime { get; set; }
        public TimeSpan EstimatedTimeRemaining { get; set; }
        public string Status { get; set; } // "Processing", "Completed", "Failed"
        public List<string> RecentMessages { get; set; } = new();
    }
}
