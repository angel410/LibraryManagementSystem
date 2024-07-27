using Library_Management_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class BorrowingRecordsController : ControllerBase
{
    private readonly LibraryContext _context;
    private readonly ILogger<BorrowingRecordsController> _logger;

    public BorrowingRecordsController(LibraryContext context, ILogger<BorrowingRecordsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost("borrow/{bookId}/patron/{patronId}")]
    public async Task<IActionResult> BorrowBook(int bookId, int patronId)
    {
        _logger.LogInformation("Attempting to borrow book with ID {BookId} for patron with ID {PatronId}", bookId, patronId);

        using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                var book = await _context.Books.FindAsync(bookId);
                var patron = await _context.Patrons.FindAsync(patronId);

                if (book == null)
                {
                    _logger.LogWarning("Book with ID {BookId} not found", bookId);
                    return NotFound(new { Message = "Book not found" });
                }

                if (patron == null)
                {
                    _logger.LogWarning("Patron with ID {PatronId} not found", patronId);
                    return NotFound(new { Message = "Patron not found" });
                }

                var borrowingRecord = new BorrowingRecord
                {
                    BookId = bookId,
                    PatronId = patronId,
                    BorrowDate = DateTime.Now
                };

                _context.BorrowingRecords.Add(borrowingRecord);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation("Book with ID {BookId} successfully borrowed by patron with ID {PatronId}", bookId, patronId);

                return Ok(borrowingRecord);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error borrowing book with ID {BookId} for patron with ID {PatronId}", bookId, patronId);
                throw;
            }
        }
    }

    [HttpPut("return/{bookId}/patron/{patronId}")]
    public async Task<IActionResult> ReturnBook(int bookId, int patronId)
    {
        _logger.LogInformation("Attempting to return book with ID {BookId} for patron with ID {PatronId}", bookId, patronId);

        var borrowingRecord = await _context.BorrowingRecords
            .FirstOrDefaultAsync(br => br.BookId == bookId && br.PatronId == patronId && br.ReturnDate == null);

        if (borrowingRecord == null)
        {
            _logger.LogWarning("Borrowing record for book with ID {BookId} and patron with ID {PatronId} not found or already returned", bookId, patronId);
            return NotFound(new { Message = "Borrowing record not found or already returned" });
        }

        borrowingRecord.ReturnDate = DateTime.Now;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Book with ID {BookId} successfully returned by patron with ID {PatronId}", bookId, patronId);

        return Ok(borrowingRecord);
    }
}
