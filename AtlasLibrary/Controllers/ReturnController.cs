using AtlasLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AtlasLibrary.Controllers
{
    public class ReturnController : Controller
    {
        private readonly HttpClient _httpClient;

        public ReturnController()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7024/");
        }

        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            var userIdString = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!int.TryParse(userIdString, out int loggedInUserId))
            {
                TempData["ErrorMessage"] = "Det gick inte att identifiera den inloggade användaren.";
                return RedirectToAction("Login", "Account");
            }

            var response = await _httpClient.GetAsync("api/Loans");

            if (!response.IsSuccessStatusCode)
            {
                return View(new List<ReturnViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();

            var loans = JsonSerializer.Deserialize<List<ReturnViewModel>>(json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<ReturnViewModel>();

            var userLoans = loans
                .Where(loan => loan.UserId == loggedInUserId)
                .ToList();

            return View(userLoans);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmReturn(int id)
        {
            var token = HttpContext.Session.GetString("JwtToken");
            var userIdString = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!int.TryParse(userIdString, out int loggedInUserId))
            {
                TempData["ErrorMessage"] = "Det gick inte att identifiera den inloggade användaren.";
                return RedirectToAction("Login", "Account");
            }

            var loanBelongsToUser = await LoanBelongsToLoggedInUser(id, loggedInUserId);

            if (!loanBelongsToUser)
            {
                TempData["ErrorMessage"] = "Du har inte behörighet att hantera detta lån.";
                return RedirectToAction("Index");
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
            var userIdString = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!int.TryParse(userIdString, out int loggedInUserId))
            {
                TempData["ErrorMessage"] = "Det gick inte att identifiera den inloggade användaren.";
                return RedirectToAction("Login", "Account");
            }

            var loanBelongsToUser = await LoanBelongsToLoggedInUser(id, loggedInUserId);

            if (!loanBelongsToUser)
            {
                TempData["ErrorMessage"] = "Du har inte behörighet att hantera detta lån.";
                return RedirectToAction("Index");
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

        private async Task<bool> LoanBelongsToLoggedInUser(int loanId, int loggedInUserId)
        {
            var response = await _httpClient.GetAsync("api/Loans");

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var json = await response.Content.ReadAsStringAsync();

            var loans = JsonSerializer.Deserialize<List<ReturnViewModel>>(json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<ReturnViewModel>();

            var loan = loans.FirstOrDefault(l => l.Id == loanId);

            if (loan == null)
            {
                return false;
            }

            return loan.UserId == loggedInUserId;
        }
    }
}