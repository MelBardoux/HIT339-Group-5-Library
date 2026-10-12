using LibrarySystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibrarySystem.Controllers
{
    [Authorize(Roles = "Admin,Manager,Reception")]
    [ApiController]
    [Route("api/[controller]")]
    public class MetadataLookupController : ControllerBase
    {
        private readonly IExternalMetadataProvider _metadataProvider;
        private readonly IMetadataEnrichmentService _enrichmentService;
        private readonly ILogger<MetadataLookupController> _logger;

        public MetadataLookupController(
            IExternalMetadataProvider metadataProvider,
            IMetadataEnrichmentService enrichmentService,
            ILogger<MetadataLookupController> logger)
        {
            _metadataProvider = metadataProvider;
            _enrichmentService = enrichmentService;
            _logger = logger;
        }

        /// <summary>
        /// Search for books by title or author
        /// </summary>
        [HttpPost("search-books")]
        public async Task<IActionResult> SearchBooks([FromBody] MetadataSearchRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return BadRequest(new { error = "Search query is required" });
            }

            try
            {
                var result = await _metadataProvider.SearchBooksAsync(request.Query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching books: {request.Query}");
                return StatusCode(500, new { error = "Error performing book search", details = ex.Message });
            }
        }

        /// <summary>
        /// Search for music by artist or album
        /// </summary>
        [HttpPost("search-music")]
        public async Task<IActionResult> SearchMusic([FromBody] MetadataSearchRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return BadRequest(new { error = "Search query is required" });
            }

            try
            {
                var result = await _metadataProvider.SearchMusicAsync(request.Query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching music: {request.Query}");
                return StatusCode(500, new { error = "Error performing music search", details = ex.Message });
            }
        }

        /// <summary>
        /// Get book metadata by ISBN
        /// </summary>
        [HttpGet("book/isbn/{isbn}")]
        public async Task<IActionResult> GetBookByIsbn(string isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn))
            {
                return BadRequest(new { error = "ISBN is required" });
            }

            try
            {
                var metadata = await _metadataProvider.GetBookMetadataByIsbnAsync(isbn);
                if (metadata == null)
                {
                    return NotFound(new { error = "No book found with that ISBN" });
                }

                return Ok(metadata);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving book metadata for ISBN: {isbn}");
                return StatusCode(500, new { error = "Error retrieving book metadata" });
            }
        }

        /// <summary>
        /// Get book metadata by author and title
        /// </summary>
        [HttpPost("book/lookup")]
        public async Task<IActionResult> GetBookByAuthorAndTitle([FromBody] BookLookupRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Author) || string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest(new { error = "Author and title are required" });
            }

            try
            {
                var metadata = await _metadataProvider.GetBookMetadataByAuthorAndTitleAsync(request.Author, request.Title);
                if (metadata == null)
                {
                    return NotFound(new { error = "No book found matching the author and title" });
                }

                return Ok(metadata);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving book metadata for {request.Author} - {request.Title}");
                return StatusCode(500, new { error = "Error retrieving book metadata" });
            }
        }

        /// <summary>
        /// Get music metadata by ISRC
        /// </summary>
        [HttpGet("music/isrc/{isrc}")]
        public async Task<IActionResult> GetMusicByIsrc(string isrc)
        {
            if (string.IsNullOrWhiteSpace(isrc))
            {
                return BadRequest(new { error = "ISRC is required" });
            }

            try
            {
                var metadata = await _metadataProvider.GetMusicMetadataByIsrcAsync(isrc);
                if (metadata == null)
                {
                    return NotFound(new { error = "No music found with that ISRC" });
                }

                return Ok(metadata);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving music metadata for ISRC: {isrc}");
                return StatusCode(500, new { error = "Error retrieving music metadata" });
            }
        }

        /// <summary>
        /// Get music metadata by artist and album
        /// </summary>
        [HttpPost("music/lookup")]
        public async Task<IActionResult> GetMusicByArtistAndAlbum([FromBody] MusicLookupRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Artist) || string.IsNullOrWhiteSpace(request.Album))
            {
                return BadRequest(new { error = "Artist and album are required" });
            }

            try
            {
                var metadata = await _metadataProvider.GetMusicMetadataByArtistAndAlbumAsync(request.Artist, request.Album);
                if (metadata == null)
                {
                    return NotFound(new { error = "No music found matching the artist and album" });
                }

                return Ok(metadata);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving music metadata for {request.Artist} - {request.Album}");
                return StatusCode(500, new { error = "Error retrieving music metadata" });
            }
        }

        /// <summary>
        /// Enrich single book with external metadata
        /// </summary>
        [HttpPost("enrich-book")]
        public async Task<IActionResult> EnrichBook([FromBody] BookLookupRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Author) || string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest(new { error = "Author and title are required" });
            }

            try
            {
                var result = await _enrichmentService.EnrichBookWithExternalMetadataAsync(
                    request.Author,
                    request.Title,
                    request.ISBN);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error enriching book: {request.Author} - {request.Title}");
                return StatusCode(500, new { error = "Error enriching book metadata" });
            }
        }

        /// <summary>
        /// Enrich single music with external metadata
        /// </summary>
        [HttpPost("enrich-music")]
        public async Task<IActionResult> EnrichMusic([FromBody] MusicLookupRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Artist) || string.IsNullOrWhiteSpace(request.Album))
            {
                return BadRequest(new { error = "Artist and album are required" });
            }

            try
            {
                var result = await _enrichmentService.EnrichMusicWithExternalMetadataAsync(
                    request.Artist,
                    request.Album,
                    request.ISRC);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error enriching music: {request.Artist} - {request.Album}");
                return StatusCode(500, new { error = "Error enriching music metadata" });
            }
        }

        /// <summary>
        /// Enrich multiple items at once
        /// </summary>
        [HttpPost("enrich-batch")]
        public async Task<IActionResult> EnrichBatch([FromBody] BatchEnrichmentRequest request)
        {
            if (request.Items == null || request.Items.Count == 0)
            {
                return BadRequest(new { error = "Items list is required" });
            }

            if (string.IsNullOrWhiteSpace(request.ItemType))
            {
                return BadRequest(new { error = "ItemType is required (Book or Music)" });
            }

            try
            {
                var results = await _enrichmentService.EnrichMultipleItemsAsync(
                    request.Items,
                    request.ItemType);

                return Ok(new
                {
                    totalCount = results.Count,
                    enrichedCount = results.Count(r => r.IsEnriched),
                    results = results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during batch enrichment");
                return StatusCode(500, new { error = "Error during batch enrichment", details = ex.Message });
            }
        }
    }

    public class MetadataSearchRequest
    {
        public string Query { get; set; }
    }

    public class BookLookupRequest
    {
        public string Author { get; set; }
        public string Title { get; set; }
        public string ISBN { get; set; }
    }

    public class MusicLookupRequest
    {
        public string Artist { get; set; }
        public string Album { get; set; }
        public string ISRC { get; set; }
    }

    public class BatchEnrichmentRequest
    {
        public string ItemType { get; set; }
        public List<CsvItemEnrichmentRequest> Items { get; set; }
    }
}
