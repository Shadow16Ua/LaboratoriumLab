using Microsoft.EntityFrameworkCore;
using WebApp.Models;

namespace WebApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Book> Books => Set<Book>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Book>(e =>
        {
            e.Property(x => x.Price).HasPrecision(10, 2);
            e.HasIndex(x => x.Isbn).IsUnique();
        });

        b.Entity<Book>().HasData(
            new Book
            {
                Id = 1,
                Title = "Solaris",
                Isbn = "978-83-08-06534-1",
                PublicationDate = new DateTime(1961, 1, 1),
                NumPages = 204,
                Price = 39.90m,
                Author = "Stanisław Lem"
            },
            new Book
            {
                Id = 2,
                Title = "Lalka",
                Isbn = "978-83-7327-000-8",
                PublicationDate = new DateTime(1890, 1, 1),
                NumPages = 720,
                Price = 29.50m,
                Author = "Bolesław Prus"
            },
            new Book
            {
                Id = 3,
                Title = "Cyberiada",
                Isbn = "978-83-08-06535-8",
                PublicationDate = new DateTime(1965, 1, 1),
                NumPages = 296,
                Price = 44.00m,
                Author = "Stanisław Lem"
            });
    }
}
