using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AtlasLibrary.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ProfileController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            var model = await LoadProfilePageModel(token);

            if (model == null)
            {
                return Content("Kunde inte hämta profilen från API.");
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(ProfilePageViewModel model)
        {
            var token = HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            var client = _httpClientFactory.CreateClient("UsersApi");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var updateData = new
            {
                namn = model.Profile.Namn,
                epost = model.Profile.Epost
            };

            var json = JsonSerializer.Serialize(updateData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync("api/users/me", content);

            var refreshedModel = await LoadProfilePageModel(token);

            if (refreshedModel == null)
            {
                return Content("Kunde inte läsa uppdaterad profildata.");
            }

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Kunde inte uppdatera profilen.";
                return View(refreshedModel);
            }

            ViewBag.Success = "Profilen uppdaterades.";
            return View(refreshedModel);
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        private async Task<ProfilePageViewModel?> LoadProfilePageModel(string token)
        {
            var usersClient = _httpClientFactory.CreateClient("UsersApi");
            usersClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var userResponse = await usersClient.GetAsync("api/users/me");

            if (!userResponse.IsSuccessStatusCode)
            {
                return null;
            }

            var userJson = await userResponse.Content.ReadAsStringAsync();

            var profile = JsonSerializer.Deserialize<ProfileResponse>(
                userJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (profile == null)
            {
                return null;
            }

            var role = HttpContext.Session.GetString("UserRole");
            profile.IsAdmin = role == "Admin";

            var loansClient = _httpClientFactory.CreateClient("LoansApi");
            var loansResponse = await loansClient.GetAsync($"api/loans/user/{profile.Id}");

            List<LoanResponse> loans = new();

            if (loansResponse.IsSuccessStatusCode)
            {
                var loansJson = await loansResponse.Content.ReadAsStringAsync();

                loans = JsonSerializer.Deserialize<List<LoanResponse>>(
                    loansJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<LoanResponse>();
            }

            return new ProfilePageViewModel
            {
                Profile = profile,
                Loans = loans
            };
        }

        public class ProfilePageViewModel
        {
            public ProfileResponse Profile { get; set; } = new();
            public List<LoanResponse> Loans { get; set; } = new();
        }

        public class ProfileResponse
        {
            public int Id { get; set; }

            [JsonPropertyName("namn")]
            public string Namn { get; set; } = string.Empty;

            [JsonPropertyName("epost")]
            public string Epost { get; set; } = string.Empty;

            public bool IsAdmin { get; set; }
        }

        public class LoanResponse
        {
            public int Id { get; set; }
            public int ItemId { get; set; }
            public int UserId { get; set; }
            public int Quantity { get; set; }
            public DateTime LoanDate { get; set; }
            public DateTime DueDate { get; set; }
            public DateTime? ReturnedDate { get; set; }
            public string Status { get; set; } = string.Empty;
        }
    }
}