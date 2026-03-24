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
                });

            //foreach (var loan in loans ?? new List<ReturnViewModel>())
            //{
            //    loan.ItemTitle = $"Bok #{loan.ItemId}";
            //}

            return View(loans ?? new List<ReturnViewModel>());
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmReturn(int id)
        {
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
