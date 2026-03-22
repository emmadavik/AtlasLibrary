using AtlasLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;


namespace AtlasLibrary.Controllers;


public class ItemsController : Controller
{
   private readonly HttpClient _httpClient;


   public ItemsController(IHttpClientFactory factory)
   {
       _httpClient = factory.CreateClient("itemsApi");
   }


   public async Task<IActionResult> Index(string category = "books", string? q = null)
   {
       string endpoint = category.ToLower() switch
       {
           "books" => string.IsNullOrWhiteSpace(q)
               ? "api/Items/books"
               : $"api/Items/books?q={q}",


           "equipment" => "api/Items/equipment",


           "reports" => "api/Items/reports",


           _ => "api/Items/books"
       };


       var items = await _httpClient.GetFromJsonAsync<List<ItemViewModel>>(endpoint)
                   ?? new List<ItemViewModel>();


       ViewBag.Category = category;
       ViewBag.Query = q;


       return View(items);
   }
   
   [HttpPost]
   public async Task<IActionResult> AddToCart(
       int itemId,
       string title,
       string author,
       string type,
       string? description,
       string imageUrl,
       string category = "books",
       string? q = null)
   {
       var payload = new
       {
           itemId,
           title,
           author,
           type,
           description = description ?? "",
           imageUrl,
           quantity = 1
       };


       var response = await _httpClient.PostAsJsonAsync("api/Cart", payload);


       var responseBody = await response.Content.ReadAsStringAsync();
       Console.WriteLine($"Cart API status: {(int)response.StatusCode}");
       Console.WriteLine($"Cart API response: {responseBody}");


       if (!response.IsSuccessStatusCode)
       {
           TempData["Error"] = "Kunde inte lägga till objektet i kundvagnen.";
       }
       else
       {
           TempData["Success"] = "Objektet lades till i kundvagnen.";
       }


       return RedirectToAction("Index", new { category, q });
   }
}

