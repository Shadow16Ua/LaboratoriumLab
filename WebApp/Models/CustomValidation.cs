using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace WebApp.Models;

public class NotInFutureAttribute : ValidationAttribute
{
    public NotInFutureAttribute() => ErrorMessage = "Data nie może być z przyszłości";

    public override bool IsValid(object? value) =>
        value is not DateTime d || d.Date <= DateTime.Today;
}

public class IsbnAttribute : ValidationAttribute
{
    public bool RequireChecksum { get; set; } = true;

    public IsbnAttribute() => ErrorMessage = "Niepoprawny format lub suma kontrolna ISBN";

    public override bool IsValid(object? value)
    {
        if (value is not string raw || string.IsNullOrWhiteSpace(raw))
            return true;

        var clean = raw.Replace("-", "").Replace(" ", "");

        // ISBN-10 (10 znaków)
        if (clean.Length == 10)
        {
            return clean.All(char.IsDigit) || (clean[..9].All(char.IsDigit) && (clean[9] == 'X' || clean[9] == 'x'));
        }

        // ISBN-13 (13 cyfr ze sprawdzeniem sumy kontrolnej)
        if (clean.Length == 13 && clean.All(char.IsDigit))
        {
            if (!RequireChecksum) return true;

            int sum = 0;
            for (int i = 0; i < 12; i++)
            {
                int digit = clean[i] - '0';
                sum += (i % 2 == 0) ? digit : digit * 3;
            }

            int checkDigit = (10 - (sum % 10)) % 10;
            return checkDigit == (clean[12] - '0');
        }

        return false;
    }
}