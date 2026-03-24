using System.Net.Http.Json;
using LibraryAdminPanel.Models;

namespace LibraryAdminPanel.Services;

public class ExternalObjectApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public ExternalObjectApiService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<List<CompletedObject>> GetCompletedObjectsAsync()
    {
        var baseUrl = _configuration["ExternalObjectsApi:BaseUrl"];
        var endpoint = _configuration["ExternalObjectsApi:CompletedObjectsEndpoint"];

        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(endpoint))
        {
            return new List<CompletedObject>();
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(baseUrl);

            var response = await client.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                return new List<CompletedObject>();
            }

            var result = await response.Content.ReadFromJsonAsync<List<CompletedObject>>();
            return result ?? new List<CompletedObject>();
        }
        catch
        {
            return new List<CompletedObject>();
        }
    }
}