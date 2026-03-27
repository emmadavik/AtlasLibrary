using Microsoft.AspNetCore.Mvc;
using items.Models;
using System.Text.Json;
using System.Net.Http.Json;

namespace items.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly IHttpClientFactory _factory;

    public ItemsController(IHttpClientFactory factory)
    {
        _factory = factory;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var allItems = new List<Item>();

        
        var booksResult = await GetBooks(null) as OkObjectResult;
        if (booksResult?.Value is List<Item> books)
            allItems.AddRange(books);

        var equipmentResult = await GetEquipment() as OkObjectResult;
        if (equipmentResult?.Value is List<Item> equipment)
            allItems.AddRange(equipment);

        var reportsResult = await GetReports() as OkObjectResult;
        if (reportsResult?.Value is List<Item> reports)
            allItems.AddRange(reports);

        return Ok(allItems);
    }

[HttpGet("books")]
public async Task<IActionResult> GetBooks([FromQuery] string? q = null)
{
    var list = new List<Item>();

    var searchTerm = string.IsNullOrWhiteSpace(q) ? "harry potter" : q;
    var url = $"https://openlibrary.org/search.json?q={Uri.EscapeDataString(searchTerm)}";

    try
    {
        using var http = new HttpClient();
        http.DefaultRequestHeaders.UserAgent.ParseAdd("AtlasLibrary/1.0");

        var response = await http.GetAsync(url);
        Console.WriteLine($"OpenLibrary status: {response.StatusCode}");

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine(json.Length);

            using var doc = JsonDocument.Parse(json);
            var docs = doc.RootElement.GetProperty("docs");

            foreach (var d in docs.EnumerateArray().Take(10))
            {
                var title = d.TryGetProperty("title", out var titleProp)
                    ? titleProp.GetString() ?? ""
                    : "";

                var author = "";
                var imageUrl = "";
                var itemId = 0;

                if (d.TryGetProperty("author_name", out var authorProp) && authorProp.ValueKind == JsonValueKind.Array && authorProp.GetArrayLength() > 0)
                {
                    author = authorProp[0].GetString() ?? "";
                }

                if (d.TryGetProperty("cover_i", out var coverProp) && coverProp.ValueKind == JsonValueKind.Number)
                {
                    itemId = coverProp.GetInt32();
                    imageUrl = $"https://covers.openlibrary.org/b/id/{itemId}-M.jpg";
                }

                if (itemId == 0)
                {
                    itemId = Math.Abs($"{title}-{author}".GetHashCode());
                }

                list.Add(new Item
                {
                    Id = itemId,
                    Title = title,
                    Author = author,
                    Description = "Ingen beskrivning tillgänglig",
                    Type = "Book",
                    IsAvailable = true,
                    ImageUrl = imageUrl
                });
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"OpenLibrary fel: {ex.Message}");
    }

    try
    {
        var client = _factory.CreateClient("adminApi");

        var adminItems = await client.GetFromJsonAsync<List<Item>>("api/items")
                         ?? new List<Item>();

        var adminBooks = adminItems
            .Where(x => !string.IsNullOrWhiteSpace(x.Type) &&
                        (x.Type.Equals("Book", StringComparison.OrdinalIgnoreCase) ||
                         x.Type.Equals("Bok", StringComparison.OrdinalIgnoreCase)))
            .ToList();

        var mappedAdminBooks = adminBooks.Select(x => new Item
        {
            Id = 100000 + x.Id,
            Title = x.Title,
            Author = x.Author,
            Description = x.Description,
            Type = x.Type,
            IsAvailable = x.IsAvailable,
            ImageUrl = x.ImageUrl ?? ""
        }).ToList();

        list.AddRange(mappedAdminBooks);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"adminApi fel: {ex.Message}");
    }

    return Ok(list);
}
    [HttpGet("equipment")]
    public async Task<IActionResult> GetEquipment()
    {
        try
        {
            var client = _factory.CreateClient("adminApi");

            var adminItems = await client.GetFromJsonAsync<List<Item>>("api/items")
                             ?? new List<Item>();

            var equipment = adminItems
                .Where(x => !string.IsNullOrWhiteSpace(x.Type) &&
                            (x.Type.Equals("Equipment", StringComparison.OrdinalIgnoreCase) ||
                             x.Type.Equals("Utrustning", StringComparison.OrdinalIgnoreCase)))
                .ToList();

            return Ok(equipment);
        }
        catch
        {
            return Ok(new List<Item>());
        }
    }

    [HttpGet("reports")]
    public async Task<IActionResult> GetReports()
    {
        try
        {
            var client = _factory.CreateClient("adminApi");

            var adminItems = await client.GetFromJsonAsync<List<Item>>("api/items")
                             ?? new List<Item>();

            var reports = adminItems
                .Where(x => !string.IsNullOrWhiteSpace(x.Type) &&
                            (x.Type.Equals("Report", StringComparison.OrdinalIgnoreCase) ||
                             x.Type.Equals("Rapport", StringComparison.OrdinalIgnoreCase)))
                .ToList();

            return Ok(reports);
        }
        catch
        {
            return Ok(new List<Item>());
        }
    }
}
