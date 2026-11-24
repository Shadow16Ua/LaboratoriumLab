using System.Globalization;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Filters;
using WebApp.Middleware;
using WebApp.Services;

var builder = WebApplication.CreateBuilder(args);

var defaultCulture = new CultureInfo("pl-PL");
CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;

// Rejestracja EF Core (SQLite) oraz serwisu książek (Lab 4)
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<IBookService, BookService>();

// Rejestracja serwisów (Lab 3: singleton statystyk, filtr akcji, globalny filtr wyjątku)
builder.Services.AddSingleton<RequestStats>();
builder.Services.AddScoped<LogActionFilter>();
builder.Services.AddControllersWithViews(o => o.Filters.Add<GlobalExceptionFilter>());

var app = builder.Build();

// Automatyczne zastosowanie migracji przy starcie aplikacji (Lab 4 – zadanie 3)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

// Własne middleware na samym początku potoku (zlicza wszystkie żądania, w tym pliki statyczne)
app.UseMiddleware<RequestTimingMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Home/HttpStatus", "?code={0}");

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

// Trasy konwencjonalne – bardziej szczegółowe przed ogólną
app.MapControllerRoute(
    name: "books-by-year",
    pattern: "books/year/{year:int:min(1450)}",
    defaults: new { controller = "Book", action = "ByYear" });

app.MapControllerRoute(
    name: "books-by-isbn",
    pattern: "books/isbn/{isbn:regex(^\\d{{13}}$)}",
    defaults: new { controller = "Book", action = "ByIsbn" });

app.MapControllerRoute(
    name: "books-archive",
    pattern: "archive/{year:int}/{month:int:range(1,12)}",
    defaults: new { controller = "Book", action = "Archive" });

app.MapControllerRoute(
    name: "docs",
    pattern: "docs/{*path}",
    defaults: new { controller = "Home", action = "Docs" });

// Trasa obszaru Admin (przed trasą default)
app.MapControllerRoute(
    name: "admin-area",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

public partial class Program { }
