using AtlasLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace AtlasLibrary.Controllers
{
    public class LoansPageController : Controller
    {
        private readonly HttpClient _httpClient;

        public LoansPageController()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7024/");
        }

        [HttpGet]
        public IActionResult Create(int? itemId)
        {
            var model = new CreateLoanViewModel
            {
                ItemId = itemId ?? 0,
                UserId = 1, // tillfälligt testvärde tills login är kopplat
                ItemTitle = itemId.HasValue ? $"Bok #{itemId}" : "Ingen bok vald",
                UserName = "Testanvändare",
                LoanDate = DateTime.Today,
                DueDate = DateTime.Today.AddDays(14),
                Quantity = 1
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateLoanViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var loanData = new
            {
                itemId = model.ItemId,
                userId = model.UserId,
                quantity = model.Quantity,
                loanDate = model.LoanDate,
                dueDate = model.DueDate,
                returnedDate = (DateTime?)null,
                status = "Pending"
            };

            var json = JsonSerializer.Serialize(loanData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/Loans", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Lånet skapades!";
                return RedirectToAction("MyLoans", new { userId = model.UserId });
            }

            ModelState.AddModelError(string.Empty, "Något gick fel när lånet skulle skapas.");

            if (string.IsNullOrWhiteSpace(model.ItemTitle))
                model.ItemTitle = $"Bok #{model.ItemId}";

            if (string.IsNullOrWhiteSpace(model.UserName))
                model.UserName = "Testanvändare";

            return View(model);
        }

        public async Task<IActionResult> MyLoans(int userId = 1)
        {
            var response = await _httpClient.GetAsync($"api/Loans/user/{userId}");

            if (!response.IsSuccessStatusCode)
            {
                return View(new List<LoanViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();

            var loans = JsonSerializer.Deserialize<List<LoanViewModel>>(json,
            new JsonSerializerOptions
            {
                 PropertyNameCaseInsensitive = true
            });

            foreach (var loan in loans!)
            {
                loan.ItemTitle = $"Bok #{loan.ItemId}";
            }

            return View(loans ?? new List<LoanViewModel>());
        }
    }
}