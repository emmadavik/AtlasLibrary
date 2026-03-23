using Microsoft.AspNetCore.Mvc;
using items.Models;
using System.Text.Json;

namespace items.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    [HttpGet("books")]
    public async Task<IActionResult> GetBooks(string q = "harry potter")
    {
        var url = $"https://openlibrary.org/search.json?q={q}";

        using var http = new HttpClient();
        var response = await http.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            return BadRequest();

        var json = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(json);
        var docs = doc.RootElement.GetProperty("docs");

        var list = new List<Item>();

        foreach (var d in docs.EnumerateArray().Take(10))
        {
            var title = d.GetProperty("title").GetString() ?? "";
            var author = "";
            var imageUrl = "";
            var itemId = 0;

            if (d.TryGetProperty("author_name", out var authorProp) && authorProp.GetArrayLength() > 0)
            {
                author = authorProp[0].GetString() ?? "";
            }

            if (d.TryGetProperty("cover_i", out var coverProp))
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

        return Ok(list);
    }

   [HttpGet("equipment")]
   public IActionResult GetEquipment()
   {
       var equipment = new List<Item>
       {
           new Item
           {
               Id = 1,
               Title = "Dell Laptop",
               Type = "Equipment",
               Description = "Bärbar dator",
               IsAvailable = true,
               ImageUrl = ""
           },
           new Item
           {
               Id = 2,
               Title = "Projektor",
               Type = "Equipment",
               Description = "Projektor för presentation",
               IsAvailable = true,
               ImageUrl = ""
           },
           new Item
           {
               Id = 3,
               Title = "HDMI kabel",
               Type = "Equipment",
               Description = "Kabel för skärm",
               IsAvailable = true,
               ImageUrl = ""
           }
       };


       return Ok(equipment);
   }


   [HttpGet("reports")]
   public IActionResult GetReports()
   {
       var reports = new List<Item>
       {
           new Item
           {
               Id = 1,
               Title = "Utlåningsrapport",
               Type = "Report",
               Description = "Rapport över utlåning",
               IsAvailable = true,
               ImageUrl = ""
           },
           new Item
           {
               Id = 2,
               Title = "Inventarierapport",
               Type = "Report",
               Description = "Lista över utrustning",
               IsAvailable = true,
               ImageUrl = ""
           }
       };


       return Ok(reports);
   }
}

