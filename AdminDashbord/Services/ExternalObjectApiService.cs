using System.Net.Http.Json;
using AdminDashbord.Models;

namespace AdminDashbord.Services;

public class ExternalObjectApiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public ExternalObjectApiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
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

        var url = $"{baseUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}";

        var result = await _httpClient.GetFromJsonAsync<List<CompletedObject>>(url);

        return result ?? new List<CompletedObject>();
    }
}