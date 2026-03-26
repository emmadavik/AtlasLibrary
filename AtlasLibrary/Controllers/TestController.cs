using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace AtlasLibrary.Controllers
{
    public class TestController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public TestController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("UsersApi");

            var loginData = new
            {
                Epost = "emil",
                Losenord = "emil"
            };

            var json = JsonSerializer.Serialize(loginData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/auth/login", content);

            var result = await response.Content.ReadAsStringAsync();

            return Content(result, "application/json");
        }
    }
}