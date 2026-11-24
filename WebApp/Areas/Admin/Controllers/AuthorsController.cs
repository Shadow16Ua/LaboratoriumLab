using Microsoft.AspNetCore.Mvc;
using WebApp.Models;

namespace WebApp.Areas.Admin.Controllers;

[Area("Admin")]
public class AuthorsController : Controller
{
    public IActionResult Index() => View(AuthorStoreMemory.Authors);

    [HttpGet]
    public IActionResult Create() => View(new Author());

    [HttpPost]
    public IActionResult Create(Author author)
    {
        if (!ModelState.IsValid)
            return View(author);

        author.Id = AuthorStoreMemory.Authors.Count > 0
            ? AuthorStoreMemory.Authors.Max(a => a.Id) + 1
            : 1;

        AuthorStoreMemory.Authors.Add(author);
        TempData["Success"] = $"Dodano autora „{author.Name}”.";
        return RedirectToAction(nameof(Index));
    }
}
