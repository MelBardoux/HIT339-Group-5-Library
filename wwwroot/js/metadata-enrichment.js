// Metadata Enrichment Module for CSV Import
// Handles lookup, search, and automatic enrichment of import data

const MetadataEnrichmentModule = (function() {
    const API_BASE = '/api/metadatalookup';
    let currentEnrichmentTask = null;
    let enrichmentResults = {};

    // Initialize event listeners
    function init() {
        console.log('MetadataEnrichmentModule initialized');
    }

    // Search for books
    async function searchBooks(query) {
        if (!query || query.trim().length === 0) {
            throw new Error('Search query is required');
        }

        try {
            const response = await fetch(`${API_BASE}/search-books`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: JSON.stringify({ query: query })
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            return await response.json();
        } catch (error) {
            console.error('Error searching books:', error);
            throw error;
        }
    }

    // Search for music
    async function searchMusic(query) {
        if (!query || query.trim().length === 0) {
            throw new Error('Search query is required');
        }

        try {
            const response = await fetch(`${API_BASE}/search-music`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: JSON.stringify({ query: query })
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            return await response.json();
        } catch (error) {
            console.error('Error searching music:', error);
            throw error;
        }
    }

    // Enrich single book
    async function enrichBook(author, title, isbn = null) {
        try {
            const response = await fetch(`${API_BASE}/enrich-book`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: JSON.stringify({
                    author: author,
                    title: title,
                    isbn: isbn
                })
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            return await response.json();
        } catch (error) {
            console.error('Error enriching book:', error);
            throw error;
        }
    }

    // Enrich single music
    async function enrichMusic(artist, album, isrc = null) {
        try {
            const response = await fetch(`${API_BASE}/enrich-music`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: JSON.stringify({
                    artist: artist,
                    album: album,
                    isrc: isrc
                })
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            return await response.json();
        } catch (error) {
            console.error('Error enriching music:', error);
            throw error;
        }
    }

    // Batch enrich multiple items
    async function enrichBatch(items, itemType, onProgress = null) {
        if (!items || items.length === 0) {
            throw new Error('Items list is required');
        }

        if (!itemType) {
            throw new Error('ItemType is required');
        }

        try {
            const response = await fetch(`${API_BASE}/enrich-batch`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: JSON.stringify({
                    itemType: itemType,
                    items: items
                })
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const result = await response.json();

            // Store results for later reference
            enrichmentResults = result;

            if (onProgress) {
                onProgress({
                    totalCount: result.totalCount,
                    enrichedCount: result.enrichedCount,
                    status: 'completed'
                });
            }

            return result;
        } catch (error) {
            console.error('Error during batch enrichment:', error);
            throw error;
        }
    }

    // Display search results in modal/dropdown
    function displaySearchResults(results, targetElement, itemType) {
        if (!results) {
            targetElement.innerHTML = '<p class="text-danger">No results found</p>';
            return;
        }

        let html = '';

        if (itemType === 'Book' && results.bookResults) {
            results.bookResults.forEach(book => {
                html += `
                    <div class="search-result-item p-2 border-bottom cursor-pointer" 
                         onclick="selectBookResult('${escapeHtml(book.title)}', '${escapeHtml(book.authorName)}')">
                        <div class="font-weight-bold">${escapeHtml(book.title)}</div>
                        <small class="text-muted">by ${escapeHtml(book.authorName)}</small>
                        <small class="badge bg-secondary ms-2">${book.confidenceScore}</small>
                    </div>
                `;
            });
        } else if (itemType === 'Music' && results.musicResults) {
            results.musicResults.forEach(music => {
                html += `
                    <div class="search-result-item p-2 border-bottom cursor-pointer"
                         onclick="selectMusicResult('${escapeHtml(music.albumTitle)}', '${escapeHtml(music.artistName)}')">
                        <div class="font-weight-bold">${escapeHtml(music.albumTitle)}</div>
                        <small class="text-muted">by ${escapeHtml(music.artistName)}</small>
                        <small class="badge bg-secondary ms-2">${music.confidenceScore}</small>
                    </div>
                `;
            });
        }

        targetElement.innerHTML = html || '<p class="text-danger">No results found</p>';
    }

    // Get cached enrichment results
    function getEnrichmentResults() {
        return enrichmentResults;
    }

    // Clear cached results
    function clearResults() {
        enrichmentResults = {};
    }

    // Format duration in milliseconds to readable string
    function formatDuration(ms) {
        if (ms < 1000) return `${ms}ms`;
        if (ms < 60000) return `${(ms / 1000).toFixed(1)}s`;
        return `${(ms / 60000).toFixed(1)}m`;
    }

    // Escape HTML special characters
    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    // Compare original vs enriched data
    function getComparativeData(original, enriched) {
        return {
            titleChanged: original.title !== enriched.title,
            authorChanged: original.author !== enriched.author,
            descriptionChanged: original.description !== enriched.description,
            newCategories: enriched.categories || [],
            newFields: {
                rating: enriched.rating,
                coverImage: enriched.coverImage,
                year: enriched.year
            }
        };
    }

    // Public API
    return {
        init: init,
        searchBooks: searchBooks,
        searchMusic: searchMusic,
        enrichBook: enrichBook,
        enrichMusic: enrichMusic,
        enrichBatch: enrichBatch,
        displaySearchResults: displaySearchResults,
        getEnrichmentResults: getEnrichmentResults,
        clearResults: clearResults,
        formatDuration: formatDuration,
        escapeHtml: escapeHtml,
        getComparativeData: getComparativeData
    };
})();

// Initialize on document ready
document.addEventListener('DOMContentLoaded', function() {
    MetadataEnrichmentModule.init();
});
