using AtlasLibrary.Models;
using AtlasLibrary.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace AtlasLibrary.Controllers;

public class ItemsController : Controller
{
    private readonly ItemsService _itemsService;
    private readonly HttpClient _httpClient;

    public ItemsController(
        ItemsService itemsService,
        IHttpClientFactory factory)
    {
        _itemsService = itemsService;
        _httpClient = factory.CreateClient("ItemsService");
    }

    public async Task<IActionResult> Index(string category = "books", string? q = null)
    {
        List<ItemViewModel> items = category.ToLower() switch
        {
            "books" => await _itemsService.GetBooks(q),
            "equipment" => await _itemsService.GetEquipment(),
            "reports" => await _itemsService.GetReports(),
            _ => await _itemsService.GetBooks(q)
        };

        ViewBag.Category = category;
        ViewBag.Query = q;

        return View(items);
    }


    [HttpPost]
    public async Task<IActionResult> AddToCart(
        int itemId,
        string title,
        string? author,
        string type,
        string? description,
        string? imageUrl,
        string category = "books",
        string? q = null)
    {
        var payload = new
        {
            itemId,
            title,
            author = author ?? "",
            type,
            description = description ?? "",
            imageUrl = imageUrl ?? "",
            quantity = 1
        };

        var response =
            await _httpClient.PostAsJsonAsync("api/cart", payload);

        return RedirectToAction("Index", new { category, q });
    }
}