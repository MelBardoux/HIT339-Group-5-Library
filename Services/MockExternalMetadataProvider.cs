namespace LibrarySystem.Services
{
    /// <summary>
    /// Represents metadata for a book from an external source
    /// </summary>
    public class ExternalBookMetadata
    {
        public string ISBN { get; set; } = string.Empty;
        public string ISBN13 { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public int? PublicationYear { get; set; }
        public int? PageCount { get; set; }
        public string Language { get; set; } = string.Empty;
        public List<string> Categories { get; set; } = new();
        public decimal? AverageRating { get; set; }
        public int? RatingCount { get; set; }
        public string CoverImageUrl { get; set; } = string.Empty;
        public string PreviewUrl { get; set; } = string.Empty;
        public string OpenLibraryId { get; set; } = string.Empty;
        public string ConfidenceScore { get; set; } = string.Empty; // High, Medium, Low
    }

    /// <summary>
    /// Represents metadata for music from an external source
    /// </summary>
    public class ExternalMusicMetadata
    {
        public string ISRC { get; set; } = string.Empty;
        public string UPC { get; set; } = string.Empty;
        public string AlbumTitle { get; set; } = string.Empty;
        public string ArtistName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public int? ReleaseYear { get; set; }
        public int? TrackCount { get; set; }
        public string RecordLabel { get; set; } = string.Empty;
        public List<string> Genres { get; set; } = new();
        public List<string> Formats { get; set; } = new();
        public decimal? AverageRating { get; set; }
        public int? RatingCount { get; set; }
        public string CoverImageUrl { get; set; } = string.Empty;
        public string PreviewAudioUrl { get; set; } = string.Empty;
        public string SpotifyId { get; set; } = string.Empty;
        public string ConfidenceScore { get; set; } = string.Empty; // High, Medium, Low
    }

    /// <summary>
    /// Search result wrapper
    /// </summary>
    public class ExternalMetadataSearchResult
    {
        public string Query { get; set; } = string.Empty;
        public int ResultCount { get; set; }
        public List<ExternalBookMetadata> BookResults { get; set; } = new();
        public List<ExternalMusicMetadata> MusicResults { get; set; } = new();
        public TimeSpan QueryTime { get; set; }
        public bool IsSuccessful { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    /// <summary>
    /// Interface for external metadata provider
    /// </summary>
    public interface IExternalMetadataProvider
    {
        Task<ExternalBookMetadata?> GetBookMetadataByIsbnAsync(string isbn);
        Task<ExternalBookMetadata?> GetBookMetadataByAuthorAndTitleAsync(string author, string title);
        Task<ExternalMusicMetadata?> GetMusicMetadataByIsrcAsync(string isrc);
        Task<ExternalMusicMetadata?> GetMusicMetadataByArtistAndAlbumAsync(string artist, string album);
        Task<ExternalMetadataSearchResult> SearchBooksAsync(string query);
        Task<ExternalMetadataSearchResult> SearchMusicAsync(string query);
    }

    /// <summary>
    /// Mock implementation of external metadata provider simulating real APIs
    /// Includes realistic delays and comprehensive metadata
    /// </summary>
    public class MockExternalMetadataProvider : IExternalMetadataProvider
    {
        private readonly ILogger<MockExternalMetadataProvider> _logger;
        private static readonly Random _random = new Random();
        private const int MIN_DELAY_MS = 800;
        private const int MAX_DELAY_MS = 3000;

        public MockExternalMetadataProvider(ILogger<MockExternalMetadataProvider> logger)
        {
            _logger = logger;
        }

        public async Task<ExternalBookMetadata?> GetBookMetadataByIsbnAsync(string isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn))
                return null;

            await SimulateApiDelay();

            return GetMockBookByIsbn(isbn);
        }

        public async Task<ExternalBookMetadata?> GetBookMetadataByAuthorAndTitleAsync(string author, string title)
        {
            if (string.IsNullOrWhiteSpace(author) || string.IsNullOrWhiteSpace(title))
                return null;

            await SimulateApiDelay();

            return GetMockBookByAuthorAndTitle(author, title);
        }

        public async Task<ExternalMusicMetadata?> GetMusicMetadataByIsrcAsync(string isrc)
        {
            if (string.IsNullOrWhiteSpace(isrc))
                return null;

            await SimulateApiDelay();

            return GetMockMusicByIsrc(isrc);
        }

        public async Task<ExternalMusicMetadata?> GetMusicMetadataByArtistAndAlbumAsync(string artist, string album)
        {
            if (string.IsNullOrWhiteSpace(artist) || string.IsNullOrWhiteSpace(album))
                return null;

            await SimulateApiDelay();

            return GetMockMusicByArtistAndAlbum(artist, album);
        }

        public async Task<ExternalMetadataSearchResult> SearchBooksAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Search query cannot be empty");

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            await SimulateApiDelay();
            stopwatch.Stop();

            var results = new ExternalMetadataSearchResult
            {
                Query = query,
                QueryTime = stopwatch.Elapsed,
                IsSuccessful = true
            };

            var mockBooks = GetMockBooksByQuery(query);
            results.BookResults = mockBooks;
            results.ResultCount = mockBooks.Count;

            _logger.LogInformation($"Book search completed: '{query}' - {mockBooks.Count} results in {stopwatch.ElapsedMilliseconds}ms");

            return results;
        }

        public async Task<ExternalMetadataSearchResult> SearchMusicAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Search query cannot be empty");

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            await SimulateApiDelay();
            stopwatch.Stop();

            var results = new ExternalMetadataSearchResult
            {
                Query = query,
                QueryTime = stopwatch.Elapsed,
                IsSuccessful = true
            };

            var mockMusic = GetMockMusicByQuery(query);
            results.MusicResults = mockMusic;
            results.ResultCount = mockMusic.Count;

            _logger.LogInformation($"Music search completed: '{query}' - {mockMusic.Count} results in {stopwatch.ElapsedMilliseconds}ms");

            return results;
        }

        private async Task SimulateApiDelay()
        {
            var delay = _random.Next(MIN_DELAY_MS, MAX_DELAY_MS);
            await Task.Delay(delay);
        }

        private ExternalBookMetadata GetMockBookByIsbn(string isbn)
        {
            // Return mock data based on ISBN
            return new ExternalBookMetadata
            {
                ISBN = isbn,
                ISBN13 = "978-" + isbn,
                Title = "Sample Book from ISBN Lookup",
                Description = "A detailed description fetched from external metadata provider based on ISBN.",
                AuthorName = "Sample Author",
                Publisher = "Sample Publishers Ltd.",
                PublicationYear = 2023,
                PageCount = 350,
                Language = "English",
                Categories = new List<string> { "Fiction", "Literature" },
                AverageRating = 4.5m,
                RatingCount = 1250,
                CoverImageUrl = "https://example.com/covers/sample-cover.jpg",
                PreviewUrl = "https://example.com/preview/sample-book",
                OpenLibraryId = "/works/OL123456W",
                ConfidenceScore = "High"
            };
        }

        private ExternalBookMetadata GetMockBookByAuthorAndTitle(string author, string title)
        {
            return new ExternalBookMetadata
            {
                ISBN = "0-123456-78-9",
                ISBN13 = "978-0-123456-78-9",
                Title = title,
                Description = $"An excellent book '{title}' by {author}. This detailed description is fetched from the external metadata provider.",
                AuthorName = author,
                Publisher = "renowned Publisher",
                PublicationYear = 2022,
                PageCount = 400,
                Language = "English",
                Categories = new List<string> { "Modern Fiction", "Award Winners" },
                AverageRating = 4.7m,
                RatingCount = 3500,
                CoverImageUrl = "https://example.com/covers/book-cover.jpg",
                PreviewUrl = "https://example.com/preview/book",
                OpenLibraryId = "/works/OL987654W",
                ConfidenceScore = "High"
            };
        }

        private ExternalMusicMetadata GetMockMusicByIsrc(string isrc)
        {
            return new ExternalMusicMetadata
            {
                ISRC = isrc,
                UPC = "123456789012",
                AlbumTitle = "Classic Album",
                ArtistName = "Famous Artist",
                Description = "A timeless collection of music from a legendary artist.",
                Publisher = "Music Label Inc.",
                ReleaseYear = 1995,
                TrackCount = 12,
                RecordLabel = "EMI Records",
                Genres = new List<string> { "Rock", "Classic Rock" },
                Formats = new List<string> { "CD", "Vinyl" },
                AverageRating = 4.8m,
                RatingCount = 5000,
                CoverImageUrl = "https://example.com/albums/classic-album.jpg",
                PreviewAudioUrl = "https://example.com/preview/album-preview.mp3",
                SpotifyId = "spotify:album:0V123456789abcdef",
                ConfidenceScore = "High"
            };
        }

        private ExternalMusicMetadata GetMockMusicByArtistAndAlbum(string artist, string album)
        {
            return new ExternalMusicMetadata
            {
                ISRC = "US1234567890",
                UPC = "654321789012",
                AlbumTitle = album,
                ArtistName = artist,
                Description = $"'{album}' is a masterpiece album by {artist}. A comprehensive collection of their finest work.",
                Publisher = "International Music Group",
                ReleaseYear = 2020,
                TrackCount = 15,
                RecordLabel = "Sony Music",
                Genres = new List<string> { "Pop", "Contemporary" },
                Formats = new List<string> { "CD", "Digital" },
                AverageRating = 4.6m,
                RatingCount = 8000,
                CoverImageUrl = "https://example.com/albums/album-art.jpg",
                PreviewAudioUrl = "https://example.com/preview/audio-sample.mp3",
                SpotifyId = "spotify:album:abcdef0123456789",
                ConfidenceScore = "High"
            };
        }

        private List<ExternalBookMetadata> GetMockBooksByQuery(string query)
        {
            var results = new List<ExternalBookMetadata>();

            // Return mock data based on query
            results.Add(new ExternalBookMetadata
            {
                ISBN = "0-123456-78-9",
                ISBN13 = "978-0-123456-78-9",
                Title = $"{query} - First Result",
                Description = $"A comprehensive overview of '{query}'. This is the top search result from the external metadata provider.",
                AuthorName = "Primary Author",
                Publisher = "Major Publisher",
                PublicationYear = 2023,
                PageCount = 425,
                Language = "English",
                Categories = new List<string> { "Non-Fiction", "Educational" },
                AverageRating = 4.8m,
                RatingCount = 5000,
                CoverImageUrl = "https://example.com/covers/result1.jpg",
                PreviewUrl = "https://example.com/preview/result1",
                OpenLibraryId = "/works/OL111111W",
                ConfidenceScore = "High"
            });

            results.Add(new ExternalBookMetadata
            {
                ISBN = "0-987654-32-1",
                ISBN13 = "978-0-987654-32-1",
                Title = $"{query} - Extended Edition",
                Description = $"A detailed exploration with additional chapters. Popular alternative to the first result.",
                AuthorName = "Secondary Author",
                Publisher = "Alternative Press",
                PublicationYear = 2023,
                PageCount = 510,
                Language = "English",
                Categories = new List<string> { "Reference", "Academic" },
                AverageRating = 4.6m,
                RatingCount = 3200,
                CoverImageUrl = "https://example.com/covers/result2.jpg",
                PreviewUrl = "https://example.com/preview/result2",
                OpenLibraryId = "/works/OL222222W",
                ConfidenceScore = "High"
            });

            results.Add(new ExternalBookMetadata
            {
                ISBN = "0-555555-55-5",
                ISBN13 = "978-0-555555-55-5",
                Title = $"Introduction to {query}",
                Description = $"A beginner-friendly introduction to the subject matter.",
                AuthorName = "Introductory Writer",
                Publisher = "Educational Publishing",
                PublicationYear = 2022,
                PageCount = 280,
                Language = "English",
                Categories = new List<string> { "Beginners", "Self-Help" },
                AverageRating = 4.4m,
                RatingCount = 2100,
                CoverImageUrl = "https://example.com/covers/result3.jpg",
                PreviewUrl = "https://example.com/preview/result3",
                OpenLibraryId = "/works/OL333333W",
                ConfidenceScore = "Medium"
            });

            return results;
        }

        private List<ExternalMusicMetadata> GetMockMusicByQuery(string query)
        {
            var results = new List<ExternalMusicMetadata>();

            results.Add(new ExternalMusicMetadata
            {
                ISRC = "US1111111111",
                UPC = "111111111111",
                AlbumTitle = $"{query} - Greatest Hits",
                ArtistName = "Primary Artist",
                Description = $"The definitive collection showcasing the best of {query}.",
                Publisher = "Music Corporation",
                ReleaseYear = 2023,
                TrackCount = 16,
                RecordLabel = "Universal Music",
                Genres = new List<string> { "Rock", "Pop" },
                Formats = new List<string> { "CD", "Vinyl", "Digital" },
                AverageRating = 4.9m,
                RatingCount = 12000,
                CoverImageUrl = "https://example.com/albums/greatest-hits.jpg",
                PreviewAudioUrl = "https://example.com/preview/sample1.mp3",
                SpotifyId = "spotify:album:track1111111111",
                ConfidenceScore = "High"
            });

            results.Add(new ExternalMusicMetadata
            {
                ISRC = "US2222222222",
                UPC = "222222222222",
                AlbumTitle = $"The Best of {query}",
                ArtistName = "Featured Performer",
                Description = $"Outstanding collection featuring {query} and collaborators.",
                Publisher = "International Records",
                ReleaseYear = 2022,
                TrackCount = 20,
                RecordLabel = "Warner Music",
                Genres = new List<string> { "Contemporary", "Jazz" },
                Formats = new List<string> { "CD" },
                AverageRating = 4.7m,
                RatingCount = 8500,
                CoverImageUrl = "https://example.com/albums/best-of.jpg",
                PreviewAudioUrl = "https://example.com/preview/sample2.mp3",
                SpotifyId = "spotify:album:track2222222222",
                ConfidenceScore = "High"
            });

            results.Add(new ExternalMusicMetadata
            {
                ISRC = "US3333333333",
                UPC = "333333333333",
                AlbumTitle = $"{query} Live",
                ArtistName = "Live Performer",
                Description = $"Exciting live recording capturing {query} at their best.",
                Publisher = "Live Music Records",
                ReleaseYear = 2023,
                TrackCount = 14,
                RecordLabel = "Indie Records",
                Genres = new List<string> { "Live", "Rock" },
                Formats = new List<string> { "CD", "Digital" },
                AverageRating = 4.5m,
                RatingCount = 4200,
                CoverImageUrl = "https://example.com/albums/live.jpg",
                PreviewAudioUrl = "https://example.com/preview/sample3.mp3",
                SpotifyId = "spotify:album:track3333333333",
                ConfidenceScore = "Medium"
            });

            return results;
        }
    }
}
