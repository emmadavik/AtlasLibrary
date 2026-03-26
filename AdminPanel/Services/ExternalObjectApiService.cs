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
        var endpoint = _configuration["ExternalObjectsApi:AdminReportItemsEndpoint"];

        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(endpoint))
        {
            return new List<CompletedObject>();
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{baseUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}";

            var result = await client.GetFromJsonAsync<List<CompletedObject>>(url);

            return result ?? new List<CompletedObject>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching completed objects: {ex.Message}");
            return new List<CompletedObject>();
        }
    }
}