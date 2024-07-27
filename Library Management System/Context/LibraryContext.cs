// Data/LibraryContext.cs
using Library_Management_System.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

public class LibraryContext : DbContext
{
    public LibraryContext(DbContextOptions<LibraryContext> options) : base(options) { }

    public DbSet<Book> Books { get; set; }
    public DbSet<Patron> Patrons { get; set; }
    public DbSet<BorrowingRecord> BorrowingRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BorrowingRecord>()
            .HasOne(br => br.Book)
            .WithMany()
            .HasForeignKey(br => br.BookId);

        modelBuilder.Entity<BorrowingRecord>()
            .HasOne(br => br.Patron)
            .WithMany()
            .HasForeignKey(br => br.PatronId);
    }
}
