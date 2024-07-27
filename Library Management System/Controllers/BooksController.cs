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
public class BooksController : ControllerBase
{
    private readonly LibraryContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<BooksController> _logger;

    public BooksController(LibraryContext context, IMemoryCache cache, ILogger<BooksController> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Book>>> GetBooks()
    {
        _logger.LogInformation("Getting all books");

        if (!_cache.TryGetValue("books", out List<Book> books))
        {
            _logger.LogInformation("Cache miss: fetching books from database");

            books = await _context.Books.ToListAsync();

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5));

            _cache.Set("books", books, cacheEntryOptions);

            _logger.LogInformation("Books retrieved from database and cached");
        }
        else
        {
            _logger.LogInformation("Cache hit: returning books from cache");
        }

        return books;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Book>> GetBook(int id)
    {
        _logger.LogInformation("Getting book with ID {BookId}", id);

        var book = await _context.Books.FindAsync(id);
        if (book == null)
        {
            _logger.LogWarning("Book with ID {BookId} not found", id);
            return NotFound();
        }

        _logger.LogInformation("Book with ID {BookId} retrieved", id);
        return book;
    }

    [HttpPost]
    public async Task<ActionResult<Book>> PostBook(Book book)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for new book");
            return BadRequest(ModelState);
        }

        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        _logger.LogInformation("New book added with ID {BookId}", book.Id);
        return CreatedAtAction("GetBook", new { id = book.Id }, book);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutBook(int id, Book book)
    {
        if (id != book.Id)
        {
            _logger.LogWarning("Book ID {BookId} mismatch", id);
            return BadRequest();
        }

        _context.Entry(book).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Book with ID {BookId} updated", book.Id);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Books.Any(e => e.Id == id))
            {
                _logger.LogWarning("Book with ID {BookId} not found for update", id);
                return NotFound();
            }
            else
            {
                _logger.LogError("Concurrency error while updating book with ID {BookId}", id);
                throw;
            }
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        _logger.LogInformation("Deleting book with ID {BookId}", id);

        var book = await _context.Books.FindAsync(id);
        if (book == null)
        {
            _logger.LogWarning("Book with ID {BookId} not found for deletion", id);
            return NotFound();
        }

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Book with ID {BookId} deleted", id);
        return NoContent();
    }
}
