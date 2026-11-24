using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Models;
using WebApp.Services;

namespace WebApp.Tests;

public class Lab4Tests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public Lab4Tests(WebApplicationFactory<Program> factory) => _factory = factory;

    private static (SqliteConnection Connection, AppDbContext Db) CreateInMemoryDb()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(conn)
            .Options;
        var db = new AppDbContext(options);
        db.Database.EnsureCreated();
        return (conn, db);
    }

    [Fact]
    public async Task BookService_CrudAndUniqueness_Works()
    {
        var (conn, db) = CreateInMemoryDb();
        await using var _ = conn;
        await using var __ = db;
        var svc = new BookService(db);

        // Seeded data exists
        Assert.Equal(3, await svc.CountAsync());
        Assert.True(await svc.IsbnExistsAsync("978-83-08-06534-1"));
        Assert.False(await svc.IsbnExistsAsync("978-83-08-06534-1", excludeId: 1));

        // Add
        var added = await svc.AddAsync(new Book
        {
            Title = "Dune",
            Isbn = "9780441172719",
            Author = "Frank Herbert",
            NumPages = 688,
            Price = 59.99m,
            PublicationDate = new DateTime(1965, 8, 1)
        });
        Assert.True(added.Id > 0);
        Assert.Equal(4, await svc.CountAsync());

        // Update
        added.Price = 49.99m;
        Assert.True(await svc.UpdateAsync(added));
        var loaded = await svc.GetByIdAsync(added.Id);
        Assert.Equal(49.99m, loaded?.Price);

        // Delete
        Assert.True(await svc.DeleteAsync(added.Id));
        Assert.Null(await svc.GetByIdAsync(added.Id));
    }

    [Fact]
    public async Task BookService_PaginationSearchSortAndAsNoTracking_Works()
    {
        var (conn, db) = CreateInMemoryDb();
        await using var _ = conn;
        await using var __ = db;
        var svc = new BookService(db);

        // Add 12 books (total 15 -> 3 pages at pageSize=5)
        for (int i = 1; i <= 12; i++)
        {
            await svc.AddAsync(new Book
            {
                Title = $"Book {i:D2}",
                Isbn = $"97800000000{i:D2}",
                Author = "Tester",
                NumPages = 100 + i,
                Price = 10m + i,
                PublicationDate = new DateTime(2000 + i, 1, 1)
            });
        }

        db.ChangeTracker.Clear();

        var page1 = await svc.GetPageAsync(page: 1, pageSize: 5);
        Assert.Equal(15, page1.TotalCount);
        Assert.Equal(3, page1.TotalPages);
        Assert.Equal(5, page1.Items.Count);
        // AsNoTracking -> nothing tracked in ChangeTracker
        Assert.Empty(db.ChangeTracker.Entries());

        // Out-of-range page does not throw
        var page99 = await svc.GetPageAsync(page: 99, pageSize: 5);
        Assert.Empty(page99.Items);
        Assert.Equal(3, page99.TotalPages);

        // Search by ISBN
        var byIsbn = await svc.GetPageAsync(1, 5, search: "978-83-7327-000-8");
        Assert.Single(byIsbn.Items);
        Assert.Equal("Lalka", byIsbn.Items[0].Title);

        // Sort by price descending
        var byPriceDesc = await svc.GetPageAsync(1, 5, sort: "price", dir: "desc");
        Assert.Equal("Cyberiada", byPriceDesc.Items[0].Title); // 44.00m is highest
    }

    [Theory]
    [InlineData("/Book")]
    [InlineData("/Book?page=99&pageSize=5")]
    [InlineData("/Book?search=Solaris")]
    [InlineData("/Book?sort=price&dir=desc")]
    [InlineData("/Book/Edit/1")]
    [InlineData("/Book/Delete/1")]
    public async Task BookCrudEndpoints_ReturnOk(string url)
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
