using System.Text;
using Microsoft.AspNetCore.Mvc;
using WebApp.Filters;
using WebApp.Models;

namespace WebApp.Controllers;

[ServiceFilter(typeof(LogActionFilter))]
[MeasureTime(Order = 2)]
public class BookController : Controller
{
    public IActionResult Hello() => Content("Witaj w BookStore!", "text/plain; charset=utf-8");

    public IActionResult Status(int code = 200) => StatusCode(code);

    [HttpGet("Book/Details/{id:int}")]
    public IActionResult Details(int id)
    {
        var book = BookStoreMemory.Books.FirstOrDefault(b => b.Id == id);
        return book is null ? NotFound() : View(book);
    }

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

    public IActionResult Health()
    {
        return Json(new { status = "ok", time = DateTime.Now });
    }

    public IActionResult Index()
    {
        return View(BookStoreMemory.Books);
    }

    public Task<IActionResult> ByYear(int year)
    {
        var books = BookStoreMemory.Books
            .Where(b => b.PublicationDate?.Year == year)
            .ToList();
        ViewData["Title"] = $"Książki z roku {year}";
        return Task.FromResult<IActionResult>(View("Index", books));
    }

    public Task<IActionResult> ByIsbn(string isbn)
    {
        var normalized = isbn.Replace("-", "").Replace(" ", "");
        var book = BookStoreMemory.Books.FirstOrDefault(b =>
            b.Isbn.Replace("-", "").Replace(" ", "").Equals(normalized, StringComparison.OrdinalIgnoreCase));
        IActionResult result = book is null
            ? NotFound()
            : RedirectToAction(nameof(Details), new { id = book.Id });
        return Task.FromResult(result);
    }

    public IActionResult Archive(int year, int month)
    {
        var books = BookStoreMemory.Books
            .Where(b => b.PublicationDate?.Year == year)
            .ToList();
        ViewData["Title"] = $"Archiwum: {year}/{month:D2}";
        return View("Index", books);
    }

    public IActionResult Boom() => throw new InvalidOperationException("Celowy wyjątek");

    [HttpGet]
    public IActionResult Create() => View(new BookFormViewModel());

    [HttpPost]
    public IActionResult Create(BookFormViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        BookStoreMemory.Books.Add(new Book
        {
            Id = BookStoreMemory.Books.Count > 0 ? BookStoreMemory.Books.Max(b => b.Id) + 1 : 1,
            Title = vm.Title,
            Isbn = vm.Isbn,
            PublicationDate = vm.PublicationDate,
            NumPages = vm.NumPages,
            Price = vm.Price,
            Author = vm.Author
        });

        TempData["Success"] = $"Dodano książkę „{vm.Title}”.";
        return RedirectToAction(nameof(Index));
    }
}
