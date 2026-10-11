# Metadata Enrichment Integration Guide

## Quick Overview

The External Metadata Provider enables you to enrich your CSV import data with external metadata before importing into the library system.

## Workflow

### Step 1: Access Import Feature
1. Go to Dashboard → "Bulk Import" tile
2. Or navigate to `/Import`

### Step 2: Upload and Preview
1. Select Item Type (Book or Music)
2. Select Branch
3. Upload CSV file
4. Review preview

### Step 3: Enrich Data (NEW!)
During the preview step, you can now choose to enrich items with external metadata:

**Option A: Automatic Enrichment**
- Click "Enrich All Items" button
- System queries external APIs for each row
- Shows enriched data with comparison view

**Option B: Manual Lookup**
- Click "Lookup" on specific row
- Search external sources
- Select best match and apply

### Step 4: Review Enriched Data
- Compare original vs. enriched values
- See coverage percentage (% items enriched)
- Review confidence scores
- Accept or reject enriched data

### Step 5: Confirm Import
- Proceed with enriched data
- Or discard enrichment and import original

## What Gets Enriched?

### Books
- ✅ Title normalization
- ✅ Author name verification
- ✅ Publication year
- ✅ ISBN/ISBN-13
- ✅ Categories/Genres
- ✅ Description
- ✅ Rating information
- ✅ Cover image URL
- ✅ Page count
- ✅ Publisher information

### Music
- ✅ Album title normalization
- ✅ Artist name verification
- ✅ Release year
- ✅ ISRC code
- ✅ Genres
- ✅ Format information
- ✅ Description
- ✅ Rating information
- ✅ Cover art URL
- ✅ Track count
- ✅ Record label

## API Usage

### Direct API Calls

For advanced users, you can call the metadata APIs directly:

```bash
# Search for books
curl -X POST http://localhost/api/metadatalookup/search-books \
  -H "Content-Type: application/json" \
  -d '{"query": "The Great Gatsby"}'

# Enrich a book
curl -X POST http://localhost/api/metadatalookup/enrich-book \
  -H "Content-Type: application/json" \
  -d '{
	"author": "F. Scott Fitzgerald",
	"title": "The Great Gatsby",
	"isbn": null
  }'

# Batch enrich
curl -X POST http://localhost/api/metadatalookup/enrich-batch \
  -H "Content-Type: application/json" \
  -d '{
	"itemType": "Book",
	"items": [
	  {"rowNumber": 1, "authorOrArtist": "Author1", "titleOrAlbum": "Title1"},
	  {"rowNumber": 2, "authorOrArtist": "Author2", "titleOrAlbum": "Title2"}
	]
  }'
```

## Data Source Mapping

| Item Type | Data Sources |
|-----------|--------------|
| **Books** | Open Library, OpenShelves, Google Books, Project Gutenberg |
| **Music** | Spotify, MusicBrainz, AllMusic, Discogs |

## Features

### Search Functionality
- Full-text search across title, author, artist, album
- Returns ranked results by relevance
- Multiple match options to choose from

### Enrichment Strategy
- **Primary lookup:** By ISBN (books) or ISRC (music)
- **Secondary lookup:** By author/artist + title/album
- **Fallback:** Returns best matching record if available

### Confidence Scores
- **High:** Exact match with multiple confirmations
- **Medium:** Good match with minor variations
- **Low:** Partial match or limited data

### Error Recovery
- Graceful fallback if enrichment fails
- Original CSV data preserved (not replaced)
- Partial enrichment allowed (enrich what's possible)

## Example Scenarios

### Scenario 1: Basic Book Import with Auto-Enrichment
```
1. Upload CSV: books.csv
   - Title: "the great gatsby"
   - Author: "scott fitzgerald"
   - Year: (blank)

2. System enriches:
   - Title: "The Great Gatsby" ✓
   - Author: "F. Scott Fitzgerald" ✓
   - Year: 1925 ✓
   - ISBN: 978-0-743-27356-2 ✓
   - Categories: ["Fiction", "Classic Literature"] ✓
   - Rating: 4.5 stars ✓
   - Confidence: High ✓

3. User reviews and imports
   - All 50 books successfully enriched
   - Imports with complete metadata
```

### Scenario 2: Music Import with Manual Selection
```
1. Upload CSV: albums.csv
   - Album: "Abbey Road"
   - Artist: "Beatles"

2. System searches and finds 3 matches
   - Option 1: Abbey Road - The Beatles (1969) ✓ High confidence
   - Option 2: Abbey Road - Paul McCartney (2012)
   - Option 3: Abbey Road Remaster - Beatles (2009)

3. User selects Option 1

4. Enriched data applied:
   - Album title: "Abbey Road"
   - Artist: "The Beatles"
   - Year: 1969
   - Label: Apple Records
   - Tracks: 17
   - Genres: ["Rock", "Pop"]
   - Rating: 4.8 stars
```

### Scenario 3: Partial Enrichment Fallback
```
1. Upload CSV with typo: "Lord of the Rings: Fellowship"
2. Primary lookup fails (title mismatch)
3. Secondary lookup finds partial match
4. System returns best guess with Medium confidence
5. User can accept or try manual search
```

## Performance Tips

1. **Batch Processing:** Enrich multiple items together (API optimized)
2. **Timing:** Enrichment takes 1-3 seconds per item (be patient)
3. **Partial Enrichment:** You can enrich only certain items
4. **Caching:** Repeated searches use cached results

## Troubleshooting

### "No results found"
- Try simpler search query
- Verify spelling and formatting
- Some items may not be in external sources

### "Enrichment timed out"
- System is busy, retry after a minute
- Try enriching fewer items at once

### "Confidence score is Low"
- Multiple matches found, manually select preferred
- Data may be incomplete in external source

### "Enrichment failed"
- Original data is preserved, can retry
- Contact admin if persistent failures

## Best Practices

1. **Always review enriched data** before importing
2. **Start with small batches** (10-20 items)
3. **High confidence enrichments** are safe to auto-accept
4. **Check for title/author changes** in comparison view
5. **Provide feedback** if enrichment is incorrect

## Advanced Configuration

For administrators, metadata enrichment can be customized:

**appsettings.json:**
```json
{
  "MetadataProvider": {
	"Enabled": true,
	"CacheResults": true,
	"MinimumConfidenceLevel": "Medium",
	"TimeoutSeconds": 30,
	"MaxBatchSize": 100,
	"DefaultSources": ["OpenLibrary", "Spotify"]
  }
}
```

## Supported Item Types

| Type | Search | Lookup | Enrich | Batch |
|------|--------|--------|--------|-------|
| **Books** | ✅ | ✅ | ✅ | ✅ |
| **Music** | ✅ | ✅ | ✅ | ✅ |
| **Toys** | ⏳ Planned | ⏳ Planned | ⏳ Planned | ⏳ Planned |

## Access Control

Only these roles can access metadata enrichment:
- ✅ Admin
- ✅ Manager  
- ✅ Reception Staff

## API Response Times

- Search: 800ms - 3000ms
- Single Lookup: 800ms - 3000ms
- Single Enrichment: 800ms - 3000ms
- Batch Enrichment: Sequential (1-3s per item)

## Limitations & Future Enhancements

**Current Limitations:**
- Mock data (not connected to real APIs)
- Sequential processing (not parallel)
- No image download support
- Limited to English language

**Planned Enhancements:**
- Real API integrations
- Parallel batch processing
- Local image caching
- Multi-language support
- User feedback rating system
- Advanced search filters
- Scheduled batch enrichment

---

For detailed technical documentation, see `METADATA_PROVIDER_GUIDE.md`
