using AtlasLibrary.Models;
using System.Net.Http.Json;

namespace AtlasLibrary.Services;

public class ItemsService
{
    private readonly HttpClient _httpClient;

    public ItemsService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<ItemViewModel>> GetBooks(string? q)
    {
        var endpoint = string.IsNullOrWhiteSpace(q)
            ? "api/items/books"
            : $"api/items/books?q={q}";

        var result =
            await _httpClient.GetFromJsonAsync<List<ItemViewModel>>(endpoint);

        return result ?? new List<ItemViewModel>();
    }

    public async Task<List<ItemViewModel>> GetEquipment()
    {
        var result =
            await _httpClient.GetFromJsonAsync<List<ItemViewModel>>(
                "api/items/equipment");

        return result ?? new List<ItemViewModel>();
    }

    public async Task<List<ItemViewModel>> GetReports()
    {
        var result =
            await _httpClient.GetFromJsonAsync<List<ItemViewModel>>(
                "api/items/reports");

        return result ?? new List<ItemViewModel>();
    }
}