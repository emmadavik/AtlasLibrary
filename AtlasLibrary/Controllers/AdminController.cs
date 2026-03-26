using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace AtlasLibrary.Controllers
{
    public class AdminController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            if (role != "Admin")
            {
                return Content("Du har inte behörighet att se denna sida.");
            }

            var client = _httpClientFactory.CreateClient("UsersApi");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("api/users");

            if (!response.IsSuccessStatusCode)
            {
                return Content("Kunde inte hämta användare från API.");
            }

            var json = await response.Content.ReadAsStringAsync();

            var users = JsonSerializer.Deserialize<List<AdminUserViewModel>>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (users == null)
            {
                users = new List<AdminUserViewModel>();
            }

            return View(users);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            if (role != "Admin")
            {
                return Content("Du har inte behörighet att se denna sida.");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(AdminUserCreateViewModel model)
        {
            var token = HttpContext.Session.GetString("JwtToken");
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            if (role != "Admin")
            {
                return Content("Du har inte behörighet att utföra denna åtgärd.");
            }

            var client = _httpClientFactory.CreateClient("UsersApi");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await client.PostAsJsonAsync("api/users", model);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Kunde inte skapa användaren.";
                return View(model);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var token = HttpContext.Session.GetString("JwtToken");
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            if (role != "Admin")
            {
                return Content("Du har inte behörighet att se denna sida.");
            }

            var client = _httpClientFactory.CreateClient("UsersApi");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"api/users/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return Content("Kunde inte hämta användaren.");
            }

            var json = await response.Content.ReadAsStringAsync();

            var user = JsonSerializer.Deserialize<AdminUserEditViewModel>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (user == null)
            {
                return Content("Användaren kunde inte läsas in.");
            }

            user.Losenord = string.Empty;

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(AdminUserEditViewModel model)
        {
            var token = HttpContext.Session.GetString("JwtToken");
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            if (role != "Admin")
            {
                return Content("Du har inte behörighet att utföra denna åtgärd.");
            }

            var inloggadId = await HamtaInloggadAnvandarId(token);

            if (inloggadId == model.Id && model.Roll == "User")
            {
                ViewBag.Error = "Du kan inte ändra din egen roll från Admin till User.";
                return View(model);
            }

            var client = _httpClientFactory.CreateClient("UsersApi");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var updateData = new
            {
                id = model.Id,
                namn = model.Namn,
                epost = model.Epost,
                losenord = string.IsNullOrWhiteSpace(model.Losenord) ? "" : model.Losenord,
                roll = model.Roll
            };

            var response = await client.PutAsJsonAsync($"api/users/{model.Id}", updateData);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Kunde inte uppdatera användaren.";
                return View(model);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var token = HttpContext.Session.GetString("JwtToken");
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            if (role != "Admin")
            {
                return Content("Du har inte behörighet att utföra denna åtgärd.");
            }

            var inloggadId = await HamtaInloggadAnvandarId(token);

            if (inloggadId == id)
            {
                return Content("Du kan inte ta bort ditt eget konto.");
            }

            var client = _httpClientFactory.CreateClient("UsersApi");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await client.DeleteAsync($"api/users/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return Content("Kunde inte ta bort användaren.");
            }

            return RedirectToAction("Index");
        }

        private async Task<int?> HamtaInloggadAnvandarId(string token)
        {
            var client = _httpClientFactory.CreateClient("UsersApi");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("api/users/me");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();

            var user = JsonSerializer.Deserialize<InloggadAnvandareViewModel>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return user?.Id;
        }

       

        public class InloggadAnvandareViewModel
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }
        }

        public class AdminUserViewModel
        {
            public int Id { get; set; }

            [JsonPropertyName("namn")]
            public string Namn { get; set; } = string.Empty;

            [JsonPropertyName("epost")]
            public string Epost { get; set; } = string.Empty;

            [JsonPropertyName("roll")]
            public string Roll { get; set; } = string.Empty;
        }

        public class AdminUserCreateViewModel
        {
            [JsonPropertyName("namn")]
            public string Namn { get; set; } = string.Empty;

            [JsonPropertyName("epost")]
            public string Epost { get; set; } = string.Empty;

            [JsonPropertyName("losenord")]
            public string Losenord { get; set; } = string.Empty;

            [JsonPropertyName("roll")]
            public string Roll { get; set; } = "User";
        }

        public class AdminUserEditViewModel
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("namn")]
            public string Namn { get; set; } = string.Empty;

            [JsonPropertyName("epost")]
            public string Epost { get; set; } = string.Empty;

            [JsonPropertyName("losenord")]
            public string Losenord { get; set; } = string.Empty;

            [JsonPropertyName("roll")]
            public string Roll { get; set; } = "User";
        }
    }
}