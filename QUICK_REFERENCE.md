# CSV Bulk Import - Quick Reference

## Quick Start (2 minutes)

### Access
1. Dashboard → "Bulk Import" tile, OR
2. Top navigation → "Bulk Import" link, OR
3. Direct URL: `/Import`

### Basic Steps
1. **Select Item Type**: Book or Music
2. **Select Branch**: Where items will be stored
3. **Upload CSV**: Choose your .csv file (max 5MB)
4. **Review Preview**: Check for errors
5. **Confirm**: Click "Confirm Import"

---

## CSV Template - Books

```
Name,Description,LibraryCode,AuthorName,PublicationYear,Genre
Title of Book,Brief description,XXX-0001,Author Name,2024,Genre
```

## CSV Template - Music

```
Name,Description,LibraryCode,AlbumTitle,ArtistName,ReleaseYear,Genre,Format
Album Name,Brief description,XXX-0001,Album Title,Artist Name,2024,Genre,CD
```

---

## LibraryCode Format

**REQUIRED FORMAT: `XXX-NNNN`**
- 3 uppercase letters
- 1 hyphen
- 4 digits

✅ Valid: `BOO-0001`, `MUS-0042`, `TOY-9999`
❌ Invalid: `BOO-01`, `book-0001`, `BOO_0001`

---

## Validation Checklist

Before uploading, ensure:
- ✓ CSV file is valid and not corrupted
- ✓ All LibraryCodes are unique
- ✓ All LibraryCodes follow XXX-NNNN format
- ✓ Required fields are not empty
- ✓ File size is under 5MB
- ✓ File extension is .csv
- ✓ Author/Artist names are complete
- ✓ Headers exactly match the template

---

## Common Issues & Solutions

| Issue | Solution |
|-------|----------|
| "File not found" | Select a valid .csv file |
| "LibraryCode already exists" | Use unique codes |
| "Invalid format" | Use XXX-NNNN format |
| "Missing required field" | Fill in all required columns |
| "File too large" | Keep CSV under 5MB |

---

## Sample Files

Download from the Import page:
- 📄 Sample Books CSV
- 📄 Sample Music CSV

Use these as templates for your imports.

---

## Role Access

| Role | Can Access? |
|------|-------------|
| Admin | ✅ YES |
| Manager | ✅ YES |
| Reception | ✅ YES |
| Other | ❌ NO |

---

## After Import

✅ Items created with status "Available"
✅ Assigned to selected branch
✅ Authors/Artists auto-created
✅ Genres/Formats must already exist

---

## Support

- Full guide: See `CSV_IMPORT_GUIDE.md`
- Issues: Contact system administrator
- Errors: Check preview messages

---

**Last Updated:** 2025  
**Version:** 1.0  
**Status:** Production Ready
