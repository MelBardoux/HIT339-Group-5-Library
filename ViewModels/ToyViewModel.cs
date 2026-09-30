using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using LibrarySystem.Data;
using LibrarySystem.Models;

namespace LibrarySystem.ViewModels
{
    public class ToyViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [Range(0, 18)]
        public int MinimumAge { get; set; }

        public bool BatteryRequired { get; set; }

        public List<TypeCheckboxViewModel> AvailableTypes { get; set; } = new();

        public async Task LoadTypes(ApplicationDbContext context)
        {
            AvailableTypes = await context.ToyTypes
                .Select(t => new TypeCheckboxViewModel
                {
                    Id = t.Id,
                    Name = t.Name,
                    IsSelected = false
                })
                .ToListAsync();
        }

        public async Task ReloadTypes(ApplicationDbContext context)
        {
            var selectedIds = AvailableTypes
                .Where(t => t.IsSelected)
                .Select(t => t.Id)
                .ToList();

            AvailableTypes = await context.ToyTypes
                .Select(t => new TypeCheckboxViewModel
                {
                    Id = t.Id,
                    Name = t.Name,
                    IsSelected = selectedIds.Contains(t.Id)
                })
                .ToListAsync();
        }

        public async Task<Toy> ToToy(ApplicationDbContext context)
        {
            var libraryCode = await Item.GenerateLibraryCode<Toy>(context, "TOY");

            var selectedTypeIds = AvailableTypes
                .Where(t => t.IsSelected)
                .Select(t => t.Id)
                .ToList();

            var toy = new Toy
            {
                Name = Name,
                Description = Description,
                MinimumAge = MinimumAge,
                BatteryRequired = BatteryRequired,
                Status = ItemStatus.Available,
                DateAdded = DateOnly.FromDateTime(DateTime.Now),
                LibraryCode = libraryCode,
                Types = await context.ToyTypes
                    .Where(t => selectedTypeIds.Contains(t.Id))
                    .ToListAsync()
            };

            return toy;
        }
    }

    public class TypeCheckboxViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }
    }
}