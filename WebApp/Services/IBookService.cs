using WebApp.Models;

namespace WebApp.Services;

public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public interface IBookService
{
    Task<PagedResult<Book>> GetPageAsync(int page, int pageSize, string? search = null, string? sort = null, string? dir = null);
    Task<Book?> GetByIdAsync(int id);
    Task<Book?> GetByIsbnAsync(string isbn);
    Task<List<Book>> GetByYearAsync(int year);
    Task<bool> IsbnExistsAsync(string isbn, int? excludeId = null);
    Task<Book> AddAsync(Book book);
    Task<bool> UpdateAsync(Book book);
    Task<bool> DeleteAsync(int id);
    Task<int> CountAsync();
}
