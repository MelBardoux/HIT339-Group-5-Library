using LibrarySystem.Data;
using LibrarySystem.DTOs;
using LibrarySystem.Filters;
using LibrarySystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Controllers.Api
{
    [ApiController]
    [Route("api/catalogue")]
    [TypeFilter(typeof(ApiKeyAuthorizationFilter))]
    public class CatalogueController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CatalogueController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("items")]
        public async Task<ActionResult<IEnumerable<CatalogueItemDto>>> GetItems()
        {
            var items = await _context.Items
                .AsNoTracking()
                .Where(i => // query to grab only certain items from specific filters
                    i.Status == ItemStatus.Available &&
                    !i.OnOrder)
                .OrderBy(i => i.Name)
                .Select(i => new CatalogueItemDto
                {
                    LibraryCode = i.LibraryCode,
                    Name = i.Name,
                    Type = i is Book
                        ? "Book"
                        : i is Toy
                            ? "Toy"
                            : "Music",
                    Branch = i.Branch.Name,
                    Status = i.Status.ToString()
                })
                .ToListAsync(); // intentionally keeping the other fields hidden from the public response

            return Ok(items);
        }
    }
}