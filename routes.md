# Mapa tras aplikacji (`AspLab` / `WebApp`)

Tabela wygenerowana na podstawie konfiguracji routingu w `Program.cs` oraz `IActionDescriptorCollectionProvider` (`/Home/Routes`).

## Trasy dedykowane (konwencjonalne i atrybutowe)

| Adres | Kontroler.Akcja | Metoda HTTP |
|---|---|---|
| `/books/year/{year:int:min(1450)}` | `Book.ByYear` | GET/POST |
| `/books/isbn/{isbn:regex(^\d{13}$)}` | `Book.ByIsbn` | GET/POST |
| `/archive/{year:int}/{month:int:range(1,12)}` | `Book.Archive` | GET/POST |
| `/docs/{*path}` | `Home.Docs` | GET/POST |
| `/Book/Details/{id:int}` | `Book.Details` | GET |
| `/Admin/{controller=Dashboard}/{action=Index}/{id?}` | `Admin.*` | GET/POST |
| `/{controller=Home}/{action=Index}/{id?}` | `Home.Index` (domyślna) | GET/POST |

## Wszystkie akcje kontrolerów

| Adres | Kontroler.Akcja | Metoda HTTP |
|---|---|---|
| `/Admin/Authors/Create` | `Authors.Create` | GET |
| `/Admin/Authors/Create` | `Authors.Create` | POST |
| `/Admin/Authors/Index` | `Authors.Index` | ANY (GET/POST) |
| `/Admin/Dashboard/Index` | `Dashboard.Index` | ANY (GET/POST) |
| `/Book/Archive` | `Book.Archive` | ANY (GET/POST) |
| `/Book/Boom` | `Book.Boom` | ANY (GET/POST) |
| `/Book/ByIsbn` | `Book.ByIsbn` | ANY (GET/POST) |
| `/Book/ByYear` | `Book.ByYear` | ANY (GET/POST) |
| `/Book/Create` | `Book.Create` | GET |
| `/Book/Create` | `Book.Create` | POST |
| `/Book/Details/{id:int}` | `Book.Details` | GET |
| `/Book/Echo` | `Book.Echo` | ANY (GET/POST) |
| `/Book/Health` | `Book.Health` | ANY (GET/POST) |
| `/Book/Hello` | `Book.Hello` | ANY (GET/POST) |
| `/Book/Import` | `Book.Import` | POST |
| `/Book/Index` | `Book.Index` | ANY (GET/POST) |
| `/Book/Inspect` | `Book.Inspect` | ANY (GET/POST) |
| `/Book/Square` | `Book.Square` | ANY (GET/POST) |
| `/Book/Status` | `Book.Status` | ANY (GET/POST) |
| `/Book/Sum` | `Book.Sum` | ANY (GET/POST) |
| `/Book/UserAgent` | `Book.UserAgent` | ANY (GET/POST) |
| `/Home/About` | `Home.About` | ANY (GET/POST) |
| `/Home/Age` | `Home.Age` | ANY (GET/POST) |
| `/Home/AgeForm` | `Home.AgeForm` | ANY (GET/POST) |
| `/Home/Contact` | `Home.Contact` | ANY (GET/POST) |
| `/Home/Docs` | `Home.Docs` | ANY (GET/POST) |
| `/Home/Error` | `Home.Error` | ANY (GET/POST) |
| `/Home/Hello` | `Home.Hello` | ANY (GET/POST) |
| `/Home/HttpStatus` | `Home.HttpStatus` | ANY (GET/POST) |
| `/Home/Index` | `Home.Index` | ANY (GET/POST) |
| `/Home/Privacy` | `Home.Privacy` | ANY (GET/POST) |
| `/Home/Routes` | `Home.Routes` | ANY (GET/POST) |
