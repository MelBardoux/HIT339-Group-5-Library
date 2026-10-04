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
        private readonly IConfiguration _configuration;

        public CatalogueController(
            ApplicationDbContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet("items")] // item endpoint
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

        [HttpGet("categories")] // categories endpoint
        public async Task<ActionResult<CatalogueCategoriesDto>> GetCategories()
        {
            var result = new CatalogueCategoriesDto
            {
                BookGenres = await _context.BookGenres
                    .AsNoTracking()
                    .OrderBy(g => g.Name)
                    .Select(g => g.Name)
                    .ToListAsync(),

                ToyTypes = await _context.ToyTypes
                    .AsNoTracking()
                    .OrderBy(t => t.Name)
                    .Select(t => t.Name)
                    .ToListAsync(),

                MusicGenres = await _context.MusicGenres
                    .AsNoTracking()
                    .OrderBy(g => g.Name)
                    .Select(g => g.Name)
                    .ToListAsync()
            };

            return Ok(result);
        }

        [HttpGet("status")] //status endpoint
        public ActionResult<OperatingStatusDto> GetStatus()
        {
            var status = _configuration["LibraryStatus:Status"];
            var message = _configuration["LibraryStatus:Message"];

            if (string.IsNullOrWhiteSpace(status) ||
                string.IsNullOrWhiteSpace(message))
            {
                return StatusCode(500, new
                {
                    message = "Library status configuration is missing."
                });
            }

            var result = new OperatingStatusDto
            {
                Status = status,
                Message = message
            };

            return Ok(result);
        }

    }
}