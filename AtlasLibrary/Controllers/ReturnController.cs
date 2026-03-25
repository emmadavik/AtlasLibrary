using AtlasLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AtlasLibrary.Controllers
{
    public class ReturnController : Controller
    {
        private readonly HttpClient _loansClient;
        private readonly HttpClient _usersClient;

        public ReturnController(IHttpClientFactory httpClientFactory)
        {
            _loansClient = httpClientFactory.CreateClient("LoansApi");
            _usersClient = httpClientFactory.CreateClient("UsersApi");
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

            var response = await _loansClient.GetAsync("api/Loans");

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

            _usersClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var usersResponse = await _usersClient.GetAsync("api/users");

            if (usersResponse.IsSuccessStatusCode)
            {
                var usersJson = await usersResponse.Content.ReadAsStringAsync();

                var users = JsonSerializer.Deserialize<List<UserResponse>>(usersJson,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<UserResponse>();

                foreach (var loan in loans)
                {
                    var user = users.FirstOrDefault(u => u.Id == loan.UserId);
                    loan.UserName = user?.Namn ?? $"Användare #{loan.UserId}";
                }
            }
            else
            {
                var errorText = await usersResponse.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Det gick inte att hämta användare. Status: {usersResponse.StatusCode}. Fel: {errorText}";

                foreach (var loan in loans)
                {
                    loan.UserName = $"Användare #{loan.UserId}";
                }
            }

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

            var response = await _loansClient.PutAsync($"api/Loans/{id}/confirm-return", null);

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

            var response = await _loansClient.DeleteAsync($"api/Loans/{id}");

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

        private class UserResponse
        {
            public int Id { get; set; }
            public string Namn { get; set; } = string.Empty;
            public string Epost { get; set; } = string.Empty;
            public string Roll { get; set; } = string.Empty;
        }
    }
}