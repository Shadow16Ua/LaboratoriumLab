using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using WebApp.Models;

namespace WebApp.Controllers;

public record RouteInfo(string Address, string ControllerAction, string HttpMethods);

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

    public IActionResult Docs(string? path)
    {
        ViewBag.Path = path;
        return View();
    }

    public IActionResult Routes([FromServices] IActionDescriptorCollectionProvider provider)
    {
        var routes = provider.ActionDescriptors.Items
            .OfType<ControllerActionDescriptor>()
            .Select(d =>
            {
                string address;
                if (d.AttributeRouteInfo?.Template is { } template)
                {
                    address = "/" + template.TrimStart('/');
                }
                else
                {
                    d.RouteValues.TryGetValue("area", out var area);
                    address = string.IsNullOrEmpty(area)
                        ? $"/{d.ControllerName}/{d.ActionName}"
                        : $"/{area}/{d.ControllerName}/{d.ActionName}";
                }

                var methods = d.ActionConstraints?
                    .OfType<HttpMethodActionConstraint>()
                    .SelectMany(c => c.HttpMethods)
                    .Distinct()
                    .ToList();

                var httpMethod = methods is { Count: > 0 } ? string.Join(", ", methods) : "ANY (GET/POST)";
                return new RouteInfo(address, $"{d.ControllerName}.{d.ActionName}", httpMethod);
            })
            .OrderBy(r => r.Address)
            .ThenBy(r => r.HttpMethods)
            .ToList();

        return View(routes);
    }

    public IActionResult HttpStatus(int code)
    {
        Response.StatusCode = code;
        return View(new ErrorViewModel
        {
            Code = code,
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
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
        int age = now.Year - birthYear.Value.Year;
        if (birthYear.Value.AddYears(age) > now)
        {
            age--;
        }

        ViewBag.Age = age;
        return View();
    }

    public IActionResult AgeForm()
    {
        return View();
    }
}
