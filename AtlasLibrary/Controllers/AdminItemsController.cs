using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using AtlasLibrary.Models;

namespace AtlasLibrary.Controllers;

public class AdminItemsController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly string _apiEndpoint = "api/items";

    public AdminItemsController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("LoansApi");
    }

    public async Task<IActionResult> Index()
    {
        var response = await _httpClient.GetAsync(_apiEndpoint);
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
        if (item.Type == "Utrustning")
        {
            item.Author = string.Empty;
        }

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var json = JsonSerializer.Serialize(item, options);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync(_apiEndpoint, content);
        
        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("Index");
        }
        
        return View(item);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var response = await _httpClient.GetAsync($"{_apiEndpoint}/{id}");
        var json = await response.Content.ReadAsStringAsync();
        var item = JsonSerializer.Deserialize<Item>(json, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return View(item);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, Item item)
    {
        if (item.Type == "Utrustning")
        {
            item.Author = string.Empty;
        }

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var json = JsonSerializer.Serialize(item, options);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        
        await _httpClient.PutAsync($"{_apiEndpoint}/{id}", content);
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await _httpClient.DeleteAsync($"{_apiEndpoint}/{id}");
        return RedirectToAction("Index");
    }
}