using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AtlasLibrary.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Login()
        {
            var token = HttpContext.Session.GetString("JwtToken");

            if (!string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string epost, string losenord)
        {
            var client = _httpClientFactory.CreateClient("UsersApi");

            var loginData = new
            {
                Epost = epost,
                Losenord = losenord
            };

            var json = JsonSerializer.Serialize(loginData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/auth/login", content);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Fel e-post eller lösenord";
                return View();
            }

            var result = await response.Content.ReadAsStringAsync();

            var loginResponse = JsonSerializer.Deserialize<LoginResponse>(
                result,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (loginResponse == null || string.IsNullOrEmpty(loginResponse.Token))
            {
                ViewBag.Error = "Något gick fel vid inloggningen";
                return View();
            }

            HttpContext.Session.SetString("JwtToken", loginResponse.Token);
            HttpContext.Session.SetString("UserRole", loginResponse.Roll ?? "");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", loginResponse.Token);

            var meResponse = await client.GetAsync("api/users/me");

            if (meResponse.IsSuccessStatusCode)
            {
                var meJson = await meResponse.Content.ReadAsStringAsync();

                var currentUser = JsonSerializer.Deserialize<CurrentUserResponse>(
                    meJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (currentUser != null)
                {
                    HttpContext.Session.SetString("UserId", currentUser.Id.ToString());
                }
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            var token = HttpContext.Session.GetString("JwtToken");

            if (!string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string namn, string epost, string losenord)
        {
            var client = _httpClientFactory.CreateClient("UsersApi");

            var registerData = new
            {
                namn = namn,
                epost = epost,
                losenord = losenord
            };

            var response = await client.PostAsJsonAsync("api/auth/register", registerData);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Kunde inte skapa konto";
                return View();
            }

            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        private class LoginResponse
        {
            public string Token { get; set; } = string.Empty;
            public string Roll { get; set; } = string.Empty;
        }

        private class CurrentUserResponse
        {
            public int Id { get; set; }
            public string Namn { get; set; } = string.Empty;
            public string Epost { get; set; } = string.Empty;
            public string Roll { get; set; } = string.Empty;
        }
    }
}