using Library_Management_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PatronsController : ControllerBase
{
    private readonly LibraryContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<PatronsController> _logger;

    public PatronsController(LibraryContext context, IMemoryCache cache, ILogger<PatronsController> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Patron>>> GetPatrons()
    {
        _logger.LogInformation("Fetching all patrons");

        if (!_cache.TryGetValue("Patrons", out List<Patron> patrons))
        {
            _logger.LogInformation("Cache miss: fetching patrons from database");

            patrons = await _context.Patrons.ToListAsync();
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5));
            _cache.Set("Patrons", patrons, cacheEntryOptions);

            _logger.LogInformation("Patrons retrieved from database and cached");
        }
        else
        {
            _logger.LogInformation("Cache hit: returning patrons from cache");
        }

        return patrons;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Patron>> GetPatron(int id)
    {
        _logger.LogInformation("Fetching patron with ID {PatronId}", id);

        var patron = await _context.Patrons.FindAsync(id);
        if (patron == null)
        {
            _logger.LogWarning("Patron with ID {PatronId} not found", id);
            return NotFound();
        }

        _logger.LogInformation("Patron with ID {PatronId} retrieved", id);
        return patron;
    }

    [HttpPost]
    public async Task<ActionResult<Patron>> PostPatron(Patron patron)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for new patron");
            return BadRequest(ModelState);
        }

        _context.Patrons.Add(patron);
        await _context.SaveChangesAsync();

        _logger.LogInformation("New patron added with ID {PatronId}", patron.Id);
        return CreatedAtAction("GetPatron", new { id = patron.Id }, patron);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutPatron(int id, Patron patron)
    {
        if (id != patron.Id)
        {
            _logger.LogWarning("Patron ID {PatronId} mismatch", id);
            return BadRequest();
        }

        _context.Entry(patron).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Patron with ID {PatronId} updated", patron.Id);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            if (!_context.Patrons.Any(e => e.Id == id))
            {
                _logger.LogWarning("Patron with ID {PatronId} not found for update", id);
                return NotFound();
            }
            else
            {
                _logger.LogError(ex, "Concurrency error while updating patron with ID {PatronId}", id);
                throw;
            }
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePatron(int id)
    {
        _logger.LogInformation("Deleting patron with ID {PatronId}", id);

        var patron = await _context.Patrons.FindAsync(id);
        if (patron == null)
        {
            _logger.LogWarning("Patron with ID {PatronId} not found for deletion", id);
            return NotFound();
        }

        _context.Patrons.Remove(patron);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Patron with ID {PatronId} deleted", id);
        return NoContent();
    }
}
