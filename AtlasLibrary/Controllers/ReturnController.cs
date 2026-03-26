using AtlasLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AtlasLibrary.Controllers
{
    public class ReturnController : Controller
    {
        private readonly HttpClient _httpClient;

        public ReturnController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("LoansApi");
        }

        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            var role = HttpContext.Session.GetString("UserRole");

            // Måste vara inloggad
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Du måste logga in för att komma åt sidan.";
                return RedirectToAction("Login", "Account");
            }

            // Måste vara admin
            if (string.IsNullOrEmpty(role) || role != "Admin")
            {
                TempData["ErrorMessage"] = "Du har inte behörighet att komma åt sidan.";
                return RedirectToAction("Index", "Home");
            }

            var response = await _httpClient.GetAsync("api/Loans");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Det gick inte att hämta lån.";
                return View(new List<ReturnViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();

            var loans = JsonSerializer.Deserialize<List<ReturnViewModel>>(json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<ReturnViewModel>();

            // Admin ser ALLA lån
            return View(loans);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmReturn(int id)
        {
            var token = HttpContext.Session.GetString("JwtToken");
            var role = HttpContext.Session.GetString("UserRole");

            // Måste vara inloggad
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Du måste logga in för att utföra åtgärden.";
                return RedirectToAction("Login", "Account");
            }

            // Måste vara admin
            if (string.IsNullOrEmpty(role) || role != "Admin")
            {
                TempData["ErrorMessage"] = "Du har inte behörighet att hantera detta lån.";
                return RedirectToAction("Index", "Home");
            }

            var response = await _httpClient.PutAsync($"api/Loans/{id}/confirm-return", null);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Återlämningen bekräftades.";
            }
            else
            {
                TempData["ErrorMessage"] = "Det gick inte att bekräfta återlämningen.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteLoan(int id)
        {
            var token = HttpContext.Session.GetString("JwtToken");
            var role = HttpContext.Session.GetString("UserRole");

            // Måste vara inloggad
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Du måste logga in för att utföra åtgärden.";
                return RedirectToAction("Login", "Account");
            }

            // Måste vara admin
            if (string.IsNullOrEmpty(role) || role != "Admin")
            {
                TempData["ErrorMessage"] = "Du har inte behörighet att hantera detta lån.";
                return RedirectToAction("Index", "Home");
            }

            var response = await _httpClient.DeleteAsync($"api/Loans/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Lånet togs bort.";
            }
            else
            {
                TempData["ErrorMessage"] = "Det gick inte att ta bort lånet.";
            }

            return RedirectToAction("Index");
        }
    }
}