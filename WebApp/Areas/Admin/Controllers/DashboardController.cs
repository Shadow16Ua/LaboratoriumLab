using Microsoft.AspNetCore.Mvc;
using WebApp.Middleware;
using WebApp.Models;
using WebApp.Services;

namespace WebApp.Areas.Admin.Controllers;

[Area("Admin")]
public class DashboardController : Controller
{
    private readonly RequestStats _stats;
    private readonly IBookService _books;

    public DashboardController(RequestStats stats, IBookService books)
    {
        _stats = stats;
        _books = books;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.TotalRequests = _stats.Total;
        ViewBag.TopPaths = _stats.TopPaths(5);
        ViewBag.BooksCount = await _books.CountAsync();
        ViewBag.AuthorsCount = AuthorStoreMemory.Authors.Count;
        return View();
    }
}
