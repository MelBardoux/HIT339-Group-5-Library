using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibrarySystem.Data;

namespace LibrarySystem.Controllers
{
    public class ArtistsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ArtistsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> SearchArtists(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return Json(new List<object>());
            }

            var artists = await _context.Artists
                .Where(a => a.Name.Contains(term))
                .Select(a => new { id = a.Id.ToString(), text = a.Name })
                .Take(10)
                .ToListAsync();

            return Json(artists);
        }
    }
}