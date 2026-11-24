using System.ComponentModel.DataAnnotations;
using WebApp.Models;

namespace WebApp.Tests;

public class ValidationTests
{
    [Theory]
    [InlineData("978-83-08-06534-1", true)]    // ISBN-13 z myślnikami
    [InlineData("9780306406157", true)]        // ISBN-13
    [InlineData("0306406152", true)]           // ISBN-10
    [InlineData("080442957X", true)]           // ISBN-10 z X
    [InlineData("9780306406158", false)]       // zła suma kontrolna
    [InlineData("abc", false)]
    [InlineData("12345", false)]
    public void IsbnAttribute_WithChecksum_Validates(string isbn, bool expected) =>
        Assert.Equal(expected, new IsbnAttribute { RequireChecksum = true }.IsValid(isbn));

    [Theory]
    [InlineData("8987059752", true)]           // poprawny format, nie musi mieć poprawnej sumy
    [InlineData("9780306406158", true)]
    [InlineData("12345", false)]
    [InlineData("abc", false)]
    public void IsbnAttribute_FormatOnly_Validates(string isbn, bool expected) =>
        Assert.Equal(expected, new IsbnAttribute().IsValid(isbn));

    [Fact]
    public void NotInFuture_RejectsTomorrow() =>
        Assert.False(new NotInFutureAttribute().IsValid(DateTime.Today.AddDays(1)));

    [Fact]
    public void NotInFuture_AcceptsToday() =>
        Assert.True(new NotInFutureAttribute().IsValid(DateTime.Today));

    [Fact]
    public void BookForm_EmptyTitle_FailsModelValidation()
    {
        var vm = new BookFormViewModel { Title = "", Isbn = "9780306406157", Price = 5, Author = "A" };
        var results = new List<ValidationResult>();

        var valid = Validator.TryValidateObject(vm, new ValidationContext(vm), results, validateAllProperties: true);

        Assert.False(valid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(BookFormViewModel.Title)));
    }
}
