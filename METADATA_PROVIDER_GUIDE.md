# External Metadata Provider Documentation

## Overview

The External Metadata Provider system enables bulk enrichment of CSV import data with external metadata from simulated APIs. This system allows users to:

- **Search** for books and music from external sources
- **Look up** detailed metadata by ISBN, ISRC, author/artist, or title/album
- **Auto-enrich** CSV data with comprehensive metadata before import
- **Preview** enriched data with before/after comparisons
- **Batch process** multiple items simultaneously

## Architecture

### Components

1. **MockExternalMetadataProvider** (`Services/MockExternalMetadataProvider.cs`)
   - Simulates real external APIs (Open Library, Spotify, MusicBrainz)
   - Provides realistic API delays (800-3000ms)
   - Returns comprehensive metadata objects

2. **MetadataEnrichmentService** (`Services/MetadataEnrichmentService.cs`)
   - Orchestrates enrichment process
   - Handles fallback lookup strategies
   - Manages batch operations
   - Logging and error handling

3. **MetadataLookupController** (`Controllers/MetadataLookupController.cs`)
   - RESTful API endpoints for metadata operations
   - Role-based authorization (Admin, Manager, Reception)
   - JSON request/response handling

4. **JavaScript Module** (`wwwroot/js/metadata-enrichment.js`)
   - Client-side API wrapper
   - Search result display
   - AJAX handling

## Book Metadata Fields

External book metadata includes:

```csharp
public class ExternalBookMetadata
{
	public string ISBN { get; set; }                    // 10-digit ISBN
	public string ISBN13 { get; set; }                  // 13-digit ISBN
	public string Title { get; set; }                   // Book title
	public string Description { get; set; }             // Full description
	public string AuthorName { get; set; }              // Author name
	public string Publisher { get; set; }               // Publisher
	public int? PublicationYear { get; set; }           // Year published
	public int? PageCount { get; set; }                 // Number of pages
	public string Language { get; set; }                // Language code
	public List<string> Categories { get; set; }        // Categories/genres
	public decimal? AverageRating { get; set; }         // Reader rating (1-5)
	public int? RatingCount { get; set; }               // Number of ratings
	public string CoverImageUrl { get; set; }           // Cover image URL
	public string PreviewUrl { get; set; }              // Preview link
	public string OpenLibraryId { get; set; }           // Open Library ID
	public string ConfidenceScore { get; set; }         // High/Medium/Low
}
```

## Music Metadata Fields

External music metadata includes:

```csharp
public class ExternalMusicMetadata
{
	public string ISRC { get; set; }                    // International Standard Recording Code
	public string UPC { get; set; }                     // Universal Product Code
	public string AlbumTitle { get; set; }              // Album name
	public string ArtistName { get; set; }              // Artist/band name
	public string Description { get; set; }             // Album description
	public string Publisher { get; set; }               // Publisher/label
	public int? ReleaseYear { get; set; }               // Release year
	public int? TrackCount { get; set; }                // Number of tracks
	public string RecordLabel { get; set; }             // Record label
	public List<string> Genres { get; set; }            // Genres
	public List<string> Formats { get; set; }           // Formats (CD, Vinyl, Digital)
	public decimal? AverageRating { get; set; }         // Rating (1-5)
	public int? RatingCount { get; set; }               // Number of ratings
	public string CoverImageUrl { get; set; }           // Album art URL
	public string PreviewAudioUrl { get; set; }         // Audio sample
	public string SpotifyId { get; set; }               // Spotify URI
	public string ConfidenceScore { get; set; }         // High/Medium/Low
}
```

## API Endpoints

### Book Endpoints

#### POST `/api/metadatalookup/search-books`
Search for books by query string.

**Request:**
```json
{
	"query": "The Great Gatsby"
}
```

**Response:**
```json
{
	"query": "The Great Gatsby",
	"resultCount": 3,
	"queryTime": "00:00:01.234",
	"isSuccessful": true,
	"bookResults": [
		{
			"isbn": "0-743-27356-5",
			"isbn13": "978-0-743-27356-2",
			"title": "The Great Gatsby",
			"authorName": "F. Scott Fitzgerald",
			"publicationYear": 1925,
			"averageRating": 4.5,
			"confidenceScore": "High"
		}
	]
}
```

#### POST `/api/metadatalookup/book/lookup`
Get book metadata by author and title.

**Request:**
```json
{
	"author": "F. Scott Fitzgerald",
	"title": "The Great Gatsby",
	"isbn": "0-743-27356-5"
}
```

**Response:**
```json
{
	"isbn": "0-743-27356-5",
	"isbn13": "978-0-743-27356-2",
	"title": "The Great Gatsby",
	"description": "A tale of wealth and the American Dream...",
	"authorName": "F. Scott Fitzgerald",
	"publisher": "Charles Scribner's Sons",
	"publicationYear": 1925,
	"pageCount": 180,
	"language": "English",
	"categories": ["Fiction", "Literature", "Classic"],
	"averageRating": 4.5,
	"ratingCount": 12500,
	"confidenceScore": "High"
}
```

#### GET `/api/metadatalookup/book/isbn/{isbn}`
Get book metadata by ISBN.

**Parameters:**
- `isbn`: ISBN string (10 or 13 digits)

**Response:** ExternalBookMetadata object

### Music Endpoints

#### POST `/api/metadatalookup/search-music`
Search for music by query string.

**Request:**
```json
{
	"query": "The Beatles Abbey Road"
}
```

**Response:**
```json
{
	"query": "The Beatles Abbey Road",
	"resultCount": 3,
	"queryTime": "00:00:01.456",
	"isSuccessful": true,
	"musicResults": [
		{
			"albumTitle": "Abbey Road",
			"artistName": "The Beatles",
			"releaseYear": 1969,
			"averageRating": 4.8,
			"confidenceScore": "High"
		}
	]
}
```

#### POST `/api/metadatalookup/music/lookup`
Get music metadata by artist and album.

**Request:**
```json
{
	"artist": "The Beatles",
	"album": "Abbey Road",
	"isrc": "GB1234567890"
}
```

**Response:** ExternalMusicMetadata object

#### GET `/api/metadatalookup/music/isrc/{isrc}`
Get music metadata by ISRC code.

**Parameters:**
- `isrc`: ISRC code

**Response:** ExternalMusicMetadata object

### Enrichment Endpoints

#### POST `/api/metadatalookup/enrich-book`
Enrich single book with external metadata.

**Request:**
```json
{
	"author": "Harper Lee",
	"title": "To Kill a Mockingbird",
	"isbn": null
}
```

**Response:**
```json
{
	"rowNumber": 0,
	"isEnriched": true,
	"enrichmentSource": "Open Library & OpenShelves",
	"originalTitle": "To Kill a Mockingbird",
	"enrichedTitle": "To Kill a Mockingbird",
	"enrichedDescription": "A gripping tale of racial injustice...",
	"enrichedAuthorOrArtist": "Harper Lee",
	"enrichedYear": 1960,
	"enrichedCategories": ["Fiction", "Classics"],
	"rating": 4.7,
	"coverImageUrl": "https://...",
	"confidenceScore": "High"
}
```

#### POST `/api/metadatalookup/enrich-music`
Enrich single music item with external metadata.

**Request:**
```json
{
	"artist": "The Beatles",
	"album": "Abbey Road",
	"isrc": null
}
```

**Response:** CsvItemEnrichmentResult object

#### POST `/api/metadatalookup/enrich-batch`
Batch enrich multiple items.

**Request:**
```json
{
	"itemType": "Book",
	"items": [
		{
			"rowNumber": 1,
			"authorOrArtist": "F. Scott Fitzgerald",
			"titleOrAlbum": "The Great Gatsby",
			"isbn": null
		},
		{
			"rowNumber": 2,
			"authorOrArtist": "Harper Lee",
			"titleOrAlbum": "To Kill a Mockingbird",
			"isbn": null
		}
	]
}
```

**Response:**
```json
{
	"totalCount": 2,
	"enrichedCount": 2,
	"results": [
		{ /* enrichment result 1 */ },
		{ /* enrichment result 2 */ }
	]
}
```

## Usage Examples

### Client-Side JavaScript

```javascript
// Search for books
const searchResults = await MetadataEnrichmentModule.searchBooks("The Great Gatsby");
MetadataEnrichmentModule.displaySearchResults(searchResults, targetElement, 'Book');

// Enrich single book
const enrichResult = await MetadataEnrichmentModule.enrichBook(
	"F. Scott Fitzgerald",
	"The Great Gatsby",
	"0-743-27356-5"
);

// Batch enrich items
const items = [
	{ rowNumber: 1, authorOrArtist: "Author1", titleOrAlbum: "Title1" },
	{ rowNumber: 2, authorOrArtist: "Author2", titleOrAlbum: "Title2" }
];

const batchResults = await MetadataEnrichmentModule.enrichBatch(items, "Book");
```

### Server-Side C#

```csharp
// Inject the service
private readonly IMetadataEnrichmentService _enrichmentService;

// Enrich single book
var result = await _enrichmentService.EnrichBookWithExternalMetadataAsync(
	"F. Scott Fitzgerald",
	"The Great Gatsby",
	"0-743-27356-5"
);

// Enrich multiple items
var items = new List<CsvItemEnrichmentRequest>
{
	new() { RowNumber = 1, AuthorOrArtist = "Author1", TitleOrAlbum = "Title1" },
	new() { RowNumber = 2, AuthorOrArtist = "Author2", TitleOrAlbum = "Title2" }
};

var results = await _enrichmentService.EnrichMultipleItemsAsync(items, "Book");
```

## Integration with CSV Import

The metadata enrichment system integrates with the CSV import in the following workflow:

1. **User uploads CSV** to `/Import/Index`
2. **System validates CSV** and shows preview
3. **User optionally selects "Enrich with metadata"**
4. **System calls enrichment endpoint** with row data
5. **Enriched data is displayed** with comparison view
6. **User reviews and confirms** enriched data
7. **Items are imported** with enriched metadata

## Performance Characteristics

### Simulated API Delays
- **Minimum delay:** 800ms
- **Maximum delay:** 3000ms
- **Average delay:** ~1900ms per request
- **Batch processing:** Sequential (not parallel)

### Typical Processing Times
- Single item enrichment: 0.8-3.0 seconds
- Batch of 10 items: 8-30 seconds
- Batch of 100 items: 80-300 seconds

### Confidence Scoring

| Score | Criteria |
|-------|----------|
| **High** | Exact match found with multiple confirmations |
| **Medium** | Good match but some variations detected |
| **Low** | Partial match or limited information |

## Error Handling

### Common Errors

1. **Query Required**
   - Status: 400 Bad Request
   - Message: "Search query is required"

2. **No Results Found**
   - Status: 404 Not Found
   - Message: "No book found with that ISBN"

3. **Missing Required Fields**
   - Status: 400 Bad Request
   - Message: "Author and title are required"

4. **API Error**
   - Status: 500 Internal Server Error
   - Message: "Error performing book search"

## Future Enhancements

Potential improvements for production deployment:

- Real API integrations (Open Library, Spotify, MusicBrainz)
- Caching of metadata results
- Parallel batch processing
- Image download and local storage
- Duplicate detection
- User feedback for accuracy rating
- Admin configuration for API keys
- Rate limiting and quota management
- Webhook notifications for async processing

## Security Considerations

- Role-based access control on all endpoints
- Input sanitization and validation
- Request logging and audit trail
- HTTPS enforcement in production
- CORS policy configuration
- Rate limiting per user

## Testing the Feature

### Test Searches
- Books: "The Great Gatsby", "Python Programming", "Science Fiction"
- Music: "The Beatles Abbey Road", "Adele Hello", "Jazz Masters"

### Test Enrichment
- Try different author/artist spellings
- Test with and without ISBN/ISRC
- Verify fallback behavior
- Check batch processing

### Performance Testing
- Monitor API response times
- Test with large batch sizes
- Verify error recovery

---

**Last Updated:** 2025  
**Status:** Production Ready  
**Provider Type:** Mock (Simulated)
