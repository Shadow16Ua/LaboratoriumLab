using System.Text;
using Microsoft.AspNetCore.Mvc;
using WebApp.Filters;
using WebApp.Models;
using WebApp.Services;

namespace WebApp.Controllers;

[ServiceFilter(typeof(LogActionFilter))]
[MeasureTime(Order = 2)]
public class BookController : Controller
{
    private readonly IBookService _books;

    public BookController(IBookService books) => _books = books;

    public IActionResult Hello() => Content("Witaj w BookStore!", "text/plain; charset=utf-8");

    public IActionResult Status(int code = 200) => StatusCode(code);

    public IActionResult Echo([FromQuery] string name = "nieznajomy", [FromQuery] int count = 1) =>
        Content(string.Join("\n", Enumerable.Repeat($"Cześć, {name}!", Math.Clamp(count, 1, 20))));

    public IActionResult Square(int id) => Content($"{id}^2 = {id * id}");

    [HttpPost]
    public IActionResult Import([FromBody] BookDto dto) =>
        Json(new { received = dto.Title, dto.Isbn, dto.Price, note = "Tylko demonstracja – nic nie zapisano." });

    public IActionResult Inspect()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Method: {Request.Method}");
        sb.AppendLine($"Path:   {Request.Path}{Request.QueryString}");
        sb.AppendLine($"Scheme: {Request.Scheme}, Host: {Request.Host}");
        sb.AppendLine($"IP:     {HttpContext.Connection.RemoteIpAddress}");
        sb.AppendLine("--- Nagłówki ---");
        foreach (var h in Request.Headers) sb.AppendLine($"{h.Key}: {h.Value}");
        return Content(sb.ToString(), "text/plain; charset=utf-8");
    }

    public IActionResult Sum(int a, int b) => Content($"{a + b}");

    public IActionResult UserAgent()
    {
        var ua = Request.Headers.UserAgent.ToString();
        return Content($"User-Agent: {ua}");
    }

    public IActionResult Health() => Json(new { status = "ok", time = DateTime.Now });

    public IActionResult Boom() => throw new InvalidOperationException("Celowy wyjątek");

    // =====================================================================
    // LAB 4 – pełny CRUD, stronicowanie, wyszukiwanie i sortowanie przez IBookService
    // =====================================================================

    public async Task<IActionResult> Index(int page = 1, int pageSize = 5, string? search = null, string? sort = null, string? dir = null)
    {
        var result = await _books.GetPageAsync(page, pageSize, search, sort, dir);
        ViewBag.Search = search;
        ViewBag.Sort = sort ?? "title";
        ViewBag.Dir = dir ?? "asc";
        ViewBag.PageSize = pageSize;
        return View(result);
    }

    public async Task<IActionResult> ByYear(int year)
    {
        var books = await _books.GetByYearAsync(year);
        ViewData["Title"] = $"Książki z roku {year}";
        return View("Index", new PagedResult<Book>(books, 1, Math.Max(1, books.Count), books.Count));
    }

    public async Task<IActionResult> ByIsbn(string isbn)
    {
        var book = await _books.GetByIsbnAsync(isbn);
        return book is null
            ? NotFound()
            : RedirectToAction(nameof(Details), new { id = book.Id });
    }

    public async Task<IActionResult> Archive(int year, int month)
    {
        var books = await _books.GetByYearAsync(year);
        ViewData["Title"] = $"Archiwum: {year}/{month:D2}";
        return View("Index", new PagedResult<Book>(books, 1, Math.Max(1, books.Count), books.Count));
    }

    [HttpGet("Book/Details/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var book = await _books.GetByIdAsync(id);
        return book is null ? NotFound() : View(book);
    }

    [HttpGet]
    public IActionResult Create() => View(new BookFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookFormViewModel vm)
    {
        if (ModelState.IsValid && await _books.IsbnExistsAsync(vm.Isbn))
        {
            ModelState.AddModelError(nameof(vm.Isbn), "Książka o takim numerze ISBN już istnieje.");
        }

        if (!ModelState.IsValid)
            return View(vm);

        await _books.AddAsync(new Book
        {
            Title = vm.Title.Trim(),
            Isbn = vm.Isbn.Trim(),
            PublicationDate = vm.PublicationDate,
            NumPages = vm.NumPages,
            Price = vm.Price,
            Author = vm.Author.Trim()
        });

        TempData["Success"] = $"Dodano książkę „{vm.Title}”.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var book = await _books.GetByIdAsync(id);
        if (book is null)
            return NotFound();

        return View(new BookFormViewModel
        {
            Id = book.Id,
            Title = book.Title,
            Isbn = book.Isbn,
            PublicationDate = book.PublicationDate,
            NumPages = book.NumPages,
            Price = book.Price,
            Author = book.Author
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BookFormViewModel vm)
    {
        if (id != vm.Id)
            return BadRequest();

        if (ModelState.IsValid && await _books.IsbnExistsAsync(vm.Isbn, excludeId: id))
        {
            ModelState.AddModelError(nameof(vm.Isbn), "Książka o takim numerze ISBN już istnieje.");
        }

        if (!ModelState.IsValid)
            return View(vm);

        var updated = await _books.UpdateAsync(new Book
        {
            Id = vm.Id,
            Title = vm.Title.Trim(),
            Isbn = vm.Isbn.Trim(),
            PublicationDate = vm.PublicationDate,
            NumPages = vm.NumPages,
            Price = vm.Price,
            Author = vm.Author.Trim()
        });

        if (!updated)
            return NotFound();

        TempData["Success"] = $"Zaktualizowano książkę „{vm.Title}”.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var book = await _books.GetByIdAsync(id);
        return book is null ? NotFound() : View(book);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var deleted = await _books.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        TempData["Success"] = "Usunięto książkę.";
        return RedirectToAction(nameof(Index));
    }
}
