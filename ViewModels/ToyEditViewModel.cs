using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using LibrarySystem.Data;
using LibrarySystem.Models;

namespace LibrarySystem.ViewModels
{
    public class ToyEditViewModel
    {
        public int Id { get; set; }

        public string LibraryCode { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [Range(0, 18)]
        public int MinimumAge { get; set; }

        public bool BatteryRequired { get; set; }

        public ItemStatus Status { get; set; }

        public bool OnOrder { get; set; }

        public List<TypeCheckboxViewModel> AvailableTypes { get; set; } = new();

        public async Task LoadTypes(ApplicationDbContext context, ICollection<ToyType> selectedTypes)
        {
            var selectedIds = selectedTypes.Select(t => t.Id).ToList();

            AvailableTypes = await context.ToyTypes
                .Select(t => new TypeCheckboxViewModel
                {
                    Id = t.Id,
                    Name = t.Name,
                    IsSelected = selectedIds.Contains(t.Id)
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

        public async Task UpdateToy(ApplicationDbContext context, Toy toy)
        {
            toy.Name = Name;
            toy.Description = Description;
            toy.MinimumAge = MinimumAge;
            toy.BatteryRequired = BatteryRequired;
            toy.Status = Status;
            toy.OnOrder = OnOrder;

            var selectedTypeIds = AvailableTypes
                .Where(t => t.IsSelected)
                .Select(t => t.Id)
                .ToList();

            toy.Types = await context.ToyTypes
                .Where(t => selectedTypeIds.Contains(t.Id))
                .ToListAsync();
        }
    }
}