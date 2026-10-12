# CSV Bulk Import Feature Documentation

## Overview
The CSV Bulk Import feature allows Admin, Manager, and Reception staff to import multiple books or music items into the library database in a single operation using a CSV (Comma-Separated Values) file.

## Features
- ✅ Role-based access control (Admin, Manager, Reception)
- ✅ Supports Books and Music item types
- ✅ Branch-specific imports
- ✅ Real-time preview before import
- ✅ Comprehensive validation and error reporting
- ✅ Automatic author/artist creation
- ✅ File size validation (max 5MB)
- ✅ CSV format validation

## Access
**Who can use this feature:**
- Admin users
- Manager users
- Reception staff

**How to access:**
1. Navigate to the main dashboard after login
2. Look for the "Bulk Import" tile in your role section
3. Or use the "Bulk Import" link in the top navigation bar
4. Or go directly to `/Import` URL

## CSV File Format

### Books Import Format
Your CSV file must contain the following columns with headers in the first row:

| Column | Required | Description | Example |
|--------|----------|-------------|---------|
| Name | Yes | Item name | The Great Gatsby |
| Description | Yes | Item description | A classic American novel |
| LibraryCode | Yes | Unique library code (XXX-NNNN format) | BOO-0001 |
| AuthorName | Yes | Author's full name | F. Scott Fitzgerald |
| PublicationYear | No | 4-digit publication year | 1925 |
| Genre | No | Genre(s) separated by semicolon | Fiction;Classic |

**Example Books CSV:**
```csv
Name,Description,LibraryCode,AuthorName,PublicationYear,Genre
The Great Gatsby,A classic American novel,BOO-0001,F. Scott Fitzgerald,1925,Fiction
To Kill a Mockingbird,A gripping tale of racial injustice,BOO-0002,Harper Lee,1960,Fiction
1984,A dystopian novel,BOO-0003,George Orwell,1949,Science Fiction
```

### Music Import Format
Your CSV file must contain the following columns with headers in the first row:

| Column | Required | Description | Example |
|--------|----------|-------------|---------|
| Name | Yes | Item name (usually album title) | Abbey Road |
| Description | Yes | Item description | Iconic album by The Beatles |
| LibraryCode | Yes | Unique library code (XXX-NNNN format) | MUS-0001 |
| AlbumTitle | Yes | Album title | Abbey Road |
| ArtistName | Yes | Artist or band name | The Beatles |
| ReleaseYear | No | 4-digit release year | 1969 |
| Genre | No | Genre(s) separated by semicolon | Rock;Pop |
| Format | No | Format(s) separated by semicolon | CD;Vinyl |

**Example Music CSV:**
```csv
Name,Description,LibraryCode,AlbumTitle,ArtistName,ReleaseYear,Genre,Format
Abbey Road,Iconic album,MUS-0001,Abbey Road,The Beatles,1969,Rock,CD
Thriller,Best-selling album,MUS-0002,Thriller,Michael Jackson,1982,Pop,CD
```

## Import Process

### Step 1: Select Item Type
Choose whether you're importing Books or Music items. The CSV columns required will vary based on this selection.

### Step 2: Select Branch
Choose the destination branch where these items will be added to inventory. Each item must be assigned to a branch.

### Step 3: Upload CSV File
- Click "Choose File" to select your CSV file
- Maximum file size: 5MB
- File must be in CSV format (.csv extension)

### Step 4: Preview
- The system will validate your CSV file
- A preview of the first 5 valid rows will be displayed
- Any validation errors will be highlighted
- Total rows, valid rows, and invalid rows are shown

### Step 5: Confirm Import
- Review the preview carefully
- Click "Confirm Import" to proceed
- The system will create all items in the database
- A confirmation message will indicate the number of successfully imported items

## Validation Rules

### LibraryCode Validation
- ✓ **Required** - Cannot be empty
- ✓ **Format** - Must follow XXX-NNNN pattern (3 uppercase letters, hyphen, 4 digits)
  - Valid examples: BOO-0001, MUS-0042, TOY-9999
  - Invalid examples: BOO-01, book-0001, BOO_0001
- ✓ **Uniqueness** - Must not already exist in the database
  - The system will reject duplicate library codes

### Item Name Validation
- ✓ **Required** - Cannot be empty
- ✓ **Max length** - 100 characters
- ✓ **Allowed characters** - Letters, spaces, periods, hyphens, and apostrophes

### Description Validation
- ✓ **Required** - Cannot be empty
- ✓ **Max length** - 500 characters

### Author/Artist Validation
- ✓ **Required** - Must provide an author name (for Books) or artist name (for Music)
- ✓ **Auto-creation** - If the author/artist doesn't exist, it will be created automatically

### Year Validation
- ✓ **Optional** - Can be left empty
- ✓ **Format** - Must be a valid 4-digit number if provided
- ✓ **Range** - Must be between 1450 and current year

### Genre/Format Validation
- ✓ **Optional** - Can be left empty
- ✓ **Multiple values** - Separate multiple genres/formats with semicolons (;)
- ✓ **Must exist** - Genres and formats must exist in the system before import
  - The system will skip genres/formats that don't exist

## Sample CSV Files

### Download Sample Files
Pre-formatted sample CSV files are available for download from the Import page:
- **Sample Books CSV** - Contains example book entries with proper formatting
- **Sample Music CSV** - Contains example music entries with proper formatting

You can use these as templates for your own import files.

## Error Handling

### What happens if there are errors?
1. Rows with validation errors are skipped
2. Valid rows are imported successfully
3. A summary is provided showing:
   - Total rows processed
   - Successfully imported rows
   - Number of errors
   - Detailed error messages for each invalid row

### Common Errors and Solutions

**LibraryCode already exists**
- Solution: Verify the code is unique or modify it

**Invalid LibraryCode format**
- Solution: Use XXX-NNNN format (e.g., BOO-0001)

**Missing required field**
- Solution: Ensure Name, Description, LibraryCode, and AuthorName (or AlbumTitle/ArtistName) are provided

**Author/Artist not found**
- Solution: The system will create it automatically; if it still fails, check the name for typos

**Invalid publication/release year**
- Solution: Use a 4-digit year or leave the field empty

## Tips for Successful Imports

1. **Start Small** - Test with 5-10 items first before bulk importing
2. **Verify Library Codes** - Ensure all library codes are unique and not already in the system
3. **Use Consistent Formatting** - Keep author/artist names formatted consistently
4. **Prepare Offline** - Create and validate your CSV in a spreadsheet program before importing
5. **Check Sample Files** - Download and review the sample CSV files to understand the format
6. **Backup Your Data** - Consider backing up your database before large imports
7. **Review Preview Carefully** - Always check the preview for errors before confirming

## Technical Details

### File Upload Limits
- Maximum file size: 5MB
- Supported format: CSV (.csv)
- Character encoding: UTF-8 recommended

### Database Impact
- Each item is created with status "Available"
- Items are assigned to the selected branch
- Authors/Artists are created automatically if needed
- Transactions are atomic (all or nothing per row)

### Performance
- Processing speed depends on file size and number of items
- Typical processing time: ~100 items per second
- Large imports (1000+ items) may take a few minutes

## Troubleshooting

**Q: My CSV file is rejected as invalid**
- A: Check that the file extension is .csv and the file is not corrupted

**Q: Preview shows no items even though my file has data**
- A: Verify the CSV headers match exactly (case-sensitive in some cases)

**Q: Some rows are marking as invalid**
- A: Check the error messages that appear; common issues include invalid library codes or missing required fields

**Q: The system created duplicate items**
- A: This should not happen if library codes are unique; verify codes before importing

**Q: Can I import to multiple branches at once?**
- A: No, each import is for a single branch. Run separate imports for different branches

## Support

For additional help or to report issues with the CSV import feature, contact your system administrator or support team.
