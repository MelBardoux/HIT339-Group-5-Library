using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibrarySystem.Data;

namespace LibrarySystem.Controllers
{
    public class AuthorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Authors/SearchAuthors?term=row
        public async Task<IActionResult> SearchAuthors(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return Json(new List<object>());
            }

            var authors = await _context.Authors
                .Where(a => a.Name.Contains(term))
                .OrderBy(a => a.Name)
                .Take(10)
                .Select(a => new { id = a.Name, text = a.Name })
                .ToListAsync();

            return Json(authors);
        }
    }
}