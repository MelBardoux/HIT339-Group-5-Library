using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using LibrarySystem.Data;
using LibrarySystem.Models;
using LibrarySystem.Validation;

namespace LibrarySystem.ViewModels
{
    public class MusicEditViewModel
    {
        public int Id { get; set; }

        public string LibraryCode { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [StringLength(100)]
        public string AlbumTitle { get; set; }

        [YearRange(1450, nameof(ReleaseYearUnknown))]
        public int? ReleaseYear { get; set; }

        public bool ReleaseYearUnknown { get; set; }

        public ItemStatus Status { get; set; }

        public bool OnOrder { get; set; }

        public List<string> ArtistNames { get; set; } = new();

        public List<MusicGenreCheckboxViewModel> AvailableGenres { get; set; } = new();
        public List<MusicFormatCheckboxViewModel> AvailableFormats { get; set; } = new();

        public async Task LoadGenresAndFormats(ApplicationDbContext context, ICollection<MusicGenre> selectedGenres, ICollection<MusicFormat> selectedFormats)
        {
            var selectedGenreIds = selectedGenres.Select(g => g.Id).ToList();

            AvailableGenres = await context.MusicGenres
                .Select(g => new MusicGenreCheckboxViewModel
                {
                    Id = g.Id,
                    Name = g.Name,
                    IsSelected = selectedGenreIds.Contains(g.Id)
                })
                .ToListAsync();

            var selectedFormatIds = selectedFormats.Select(f => f.Id).ToList();

            AvailableFormats = await context.MusicFormats
                .Select(f => new MusicFormatCheckboxViewModel
                {
                    Id = f.Id,
                    Name = f.Name,
                    IsSelected = selectedFormatIds.Contains(f.Id)
                })
                .ToListAsync();
        }

        public async Task ReloadGenresAndFormats(ApplicationDbContext context)
        {
            var selectedGenreIds = AvailableGenres
                .Where(g => g.IsSelected)
                .Select(g => g.Id)
                .ToList();

            AvailableGenres = await context.MusicGenres
                .Select(g => new MusicGenreCheckboxViewModel
                {
                    Id = g.Id,
                    Name = g.Name,
                    IsSelected = selectedGenreIds.Contains(g.Id)
                })
                .ToListAsync();

            var selectedFormatIds = AvailableFormats
                .Where(f => f.IsSelected)
                .Select(f => f.Id)
                .ToList();

            AvailableFormats = await context.MusicFormats
                .Select(f => new MusicFormatCheckboxViewModel
                {
                    Id = f.Id,
                    Name = f.Name,
                    IsSelected = selectedFormatIds.Contains(f.Id)
                })
                .ToListAsync();
        }

        public async Task<List<Artist>> ResolveArtists(ApplicationDbContext context)
        {
            var artists = new List<Artist>();

            foreach (var name in ArtistNames)
            {
                if (string.IsNullOrWhiteSpace(name)) continue;

                var artist = await context.Artists
                    .FirstOrDefaultAsync(a => a.Name == name);

                if (artist == null)
                {
                    artist = new Artist { Name = name };
                    context.Artists.Add(artist);
                    await context.SaveChangesAsync();
                }

                artists.Add(artist);
            }

            return artists;
        }

        public async Task UpdateMusic(ApplicationDbContext context, Music music)
        {
            var artists = await ResolveArtists(context);

            music.Name = Name;
            music.Description = Description;
            music.AlbumTitle = AlbumTitle;
            music.ReleaseYear = ReleaseYear;
            music.ReleaseYearUnknown = ReleaseYearUnknown;
            music.Status = Status;
            music.OnOrder = OnOrder;
            music.Artists = artists;

            var selectedGenreIds = AvailableGenres
                .Where(g => g.IsSelected)
                .Select(g => g.Id)
                .ToList();

            music.Genres = await context.MusicGenres
                .Where(g => selectedGenreIds.Contains(g.Id))
                .ToListAsync();

            var selectedFormatIds = AvailableFormats
                .Where(f => f.IsSelected)
                .Select(f => f.Id)
                .ToList();

            music.Formats = await context.MusicFormats
                .Where(f => selectedFormatIds.Contains(f.Id))
                .ToListAsync();
        }
    }
}