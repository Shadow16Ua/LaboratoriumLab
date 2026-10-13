using System.ComponentModel.DataAnnotations;

namespace WebApp.Models;

public class BookFormViewModel : IValidatableObject
{
    [Required(ErrorMessage = "Tytuł jest wymagany")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Tytuł: 2–200 znaków")]
    [Display(Name = "Tytuł")]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "ISBN jest wymagany"), Isbn]
    public string Isbn { get; set; } = "";

    [DataType(DataType.Date)]
    [NotInFuture]
    [Display(Name = "Data wydania")]
    public DateTime? PublicationDate { get; set; }

    [Range(1, 20000, ErrorMessage = "Liczba stron: 1–20000")]
    [Display(Name = "Liczba stron")]
    public int NumPages { get; set; } = 100;

    [Range(0.01, 10000, ErrorMessage = "Cena musi być dodatnia")]
    [Display(Name = "Cena")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Autor jest wymagany")]
    [Display(Name = "Autor")]
    public string Author { get; set; } = "";

    [Display(Name = "Kod wydawcy (np. ABC-1234)")]
    [RegularExpression(@"^[A-Z]{3}-\d{4}$", ErrorMessage = "Kod wydawcy musi mieć format ABC-1234")]
    public string? PublisherCode { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (PublicationDate.HasValue && PublicationDate.Value.Year < 1900 && Price > 100)
        {
            yield return new ValidationResult(
                "Książka sprzed 1900 r. nie może kosztować więcej niż 100 zł.",
                new[] { nameof(Price) }
            );
        }
    }
}