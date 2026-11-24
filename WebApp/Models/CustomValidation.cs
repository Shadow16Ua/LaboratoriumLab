using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace WebApp.Models;

/// <summary>Własny atrybut walidacji – data nie może być z przyszłości.</summary>
public class NotInFutureAttribute : ValidationAttribute
{
    public NotInFutureAttribute() => ErrorMessage = "Data nie może być z przyszłości";

    public override bool IsValid(object? value) =>
        value is not DateTime d || d.Date <= DateTime.Today;
}

/// <summary>
/// Własny atrybut – format ISBN (10 lub 13 cyfr, myślniki i spacje dozwolone).
/// Sprawdzanie sumy kontrolnej jest opcjonalne: [Isbn(RequireChecksum = true)].
/// </summary>
public class IsbnAttribute : ValidationAttribute
{
    public bool RequireChecksum { get; set; }

    public IsbnAttribute() => ErrorMessage = "Niepoprawny numer ISBN";

    public override bool IsValid(object? value)
    {
        if (value is not string s || string.IsNullOrWhiteSpace(s)) return true; // [Required] zadba o brak
        var d = Regex.Replace(s, "[- ]", "");
        if (!Regex.IsMatch(d, @"^(\d{10,13}|\d{9}[Xx])$")) return false;
        if (!RequireChecksum) return true;

        if (d.Length == 13)
        {
            int sum = 0;
            for (int i = 0; i < 13; i++) sum += (d[i] - '0') * (i % 2 == 0 ? 1 : 3);
            return sum % 10 == 0;
        }
        if (d.Length == 10)
        {
            int sum = 0;
            for (int i = 0; i < 10; i++) sum += (d[i] is 'X' or 'x' ? 10 : d[i] - '0') * (10 - i);
            return sum % 11 == 0;
        }
        return false; // 11–12 cyfr nie jest poprawnym ISBN
    }
}
