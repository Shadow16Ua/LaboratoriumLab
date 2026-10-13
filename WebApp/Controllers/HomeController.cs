using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;

namespace WebApp.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Hello()
    {
        return Content("Hello from ASP.NET", "text/plain; charset=UTF-8");
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult Contact()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult Age(DateTime? birthYear)
    {
        if (birthYear == null)
        {
            return BadRequest();
        }

        DateTime now = DateTime.Now;
        int Age = now.Year - birthYear.Value.Year;
        if (birthYear.Value.AddYears(Age) > now)
        {
            return View();
        }

        return null;
    }

    public IActionResult AgeForm()
    {
        return View();
    }

}
