using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Models;

namespace WebApp.Services;

public class BookService : IBookService
{
    private readonly AppDbContext _db;

    public BookService(AppDbContext db) => _db = db;

    public async Task<PagedResult<Book>> GetPageAsync(
        int page,
        int pageSize,
        string? search = null,
        string? sort = null,
        string? dir = null)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        IQueryable<Book> q = _db.Books.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            q = q.Where(b => b.Title.Contains(term) || b.Isbn.Contains(term));
        }

        var total = await q.CountAsync();

        q = (sort?.ToLowerInvariant(), dir?.ToLowerInvariant()) switch
        {
            ("price", "desc") => q.OrderByDescending(b => (double)b.Price),
            ("price", _) => q.OrderBy(b => (double)b.Price),
            ("date", "desc") => q.OrderByDescending(b => b.PublicationDate),
            ("date", _) => q.OrderBy(b => b.PublicationDate),
            ("title", "desc") => q.OrderByDescending(b => b.Title),
            _ => q.OrderBy(b => b.Title)
        };

        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Book>(items, page, pageSize, total);
    }

    public async Task<Book?> GetByIdAsync(int id) =>
        await _db.Books.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);

    public async Task<Book?> GetByIsbnAsync(string isbn)
    {
        var normalized = isbn.Replace("-", "").Replace(" ", "");
        return await _db.Books.AsNoTracking()
            .FirstOrDefaultAsync(b => b.Isbn.Replace("-", "").Replace(" ", "") == normalized);
    }

    public async Task<List<Book>> GetByYearAsync(int year) =>
        await _db.Books.AsNoTracking()
            .Where(b => b.PublicationDate.HasValue && b.PublicationDate.Value.Year == year)
            .OrderBy(b => b.Title)
            .ToListAsync();

    public async Task<bool> IsbnExistsAsync(string isbn, int? excludeId = null)
    {
        var trimmed = isbn.Trim();
        return await _db.Books.AsNoTracking()
            .AnyAsync(b => b.Isbn == trimmed && (!excludeId.HasValue || b.Id != excludeId.Value));
    }

    public async Task<Book> AddAsync(Book book)
    {
        _db.Books.Add(book);
        await _db.SaveChangesAsync();
        return book;
    }

    public async Task<bool> UpdateAsync(Book book)
    {
        var existing = await _db.Books.FindAsync(book.Id);
        if (existing is null)
            return false;

        existing.Title = book.Title;
        existing.Isbn = book.Isbn;
        existing.PublicationDate = book.PublicationDate;
        existing.NumPages = book.NumPages;
        existing.Price = book.Price;
        existing.Author = book.Author;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _db.Books.FindAsync(id);
        if (existing is null)
            return false;

        _db.Books.Remove(existing);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<int> CountAsync() =>
        await _db.Books.AsNoTracking().CountAsync();
}
