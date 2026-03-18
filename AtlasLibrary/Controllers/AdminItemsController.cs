using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using AtlasLibrary.Models;

namespace AtlasLibrary.Controllers;

public class AdminItemsController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly string _apiUrl = "http://localhost:5070/api/items";

    public AdminItemsController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task<IActionResult> Index()
    {
        var response = await _httpClient.GetAsync(_apiUrl);
        var json = await response.Content.ReadAsStringAsync();
        var items = JsonSerializer.Deserialize<List<Item>>(json, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return View(items);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Item item)
    {
        var json = JsonSerializer.Serialize(item);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        await _httpClient.PostAsync(_apiUrl, content);
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Edit(int id)
    {
        var response = await _httpClient.GetAsync($"{_apiUrl}/{id}");
        var json = await response.Content.ReadAsStringAsync();
        var item = JsonSerializer.Deserialize<Item>(json, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return View(item);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, Item item)
    {
        var json = JsonSerializer.Serialize(item);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        await _httpClient.PutAsync($"{_apiUrl}/{id}", content);
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await _httpClient.DeleteAsync($"{_apiUrl}/{id}");
        return RedirectToAction("Index");
    }
}