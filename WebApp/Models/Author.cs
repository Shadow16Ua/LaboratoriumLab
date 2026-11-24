using System.ComponentModel.DataAnnotations;

namespace WebApp.Models;

public class Author
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Imię i nazwisko autora jest wymagane")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Nazwa autora: 2–150 znaków")]
    [Display(Name = "Autor")]
    public string Name { get; set; } = "";
}

public static class AuthorStoreMemory
{
    public static readonly List<Author> Authors = new()
    {
        new() { Id = 1, Name = "Stanisław Lem" },
        new() { Id = 2, Name = "Bolesław Prus" },
        new() { Id = 3, Name = "Olga Tokarczuk" }
    };
}
