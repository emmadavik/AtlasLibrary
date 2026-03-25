using AtlasLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace AtlasLibrary.Controllers
{
    public class LoansPageController : Controller
    {
        private readonly HttpClient _itemsClient;
        private readonly HttpClient _loansClient;

        public LoansPageController(IHttpClientFactory httpClientFactory)
        {
            _itemsClient = httpClientFactory.CreateClient("ItemsService");
            _loansClient = httpClientFactory.CreateClient("LoansApi");
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            var userIdString = HttpContext.Session.GetString("UserId");
            var userName = HttpContext.Session.GetString("UserName");

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userIdString))
            {
                TempData["ErrorMessage"] = "Du måste logga in för att skapa lån.";
                return RedirectToAction("Login", "Account");
            }

            var cart = await _itemsClient.GetFromJsonAsync<List<CartItemViewModel>>("api/Cart")
                       ?? new List<CartItemViewModel>();

            if (!cart.Any())
            {
                TempData["ErrorMessage"] = "Varukorgen är tom.";
                return RedirectToAction("Index", "Cart");
            }

            var model = new CreateLoanViewModel
            {
                UserId = int.Parse(userIdString),
                UserName = userName,
                LoanDate = DateTime.Today,
                DueDate = DateTime.Today.AddDays(14),
                CartItems = cart
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateLoanViewModel model)
        {
            var token = HttpContext.Session.GetString("JwtToken");
            var userIdString = HttpContext.Session.GetString("UserId");
            var userName = HttpContext.Session.GetString("UserName");

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userIdString))
            {
                TempData["ErrorMessage"] = "Du måste logga in för att skapa lån.";
                return RedirectToAction("Login", "Account");
            }

            int currentUserId = int.Parse(userIdString);

            if (!ModelState.IsValid)
            {
                var cartFallback = await _itemsClient.GetFromJsonAsync<List<CartItemViewModel>>("api/Cart")
                                   ?? new List<CartItemViewModel>();

                model.UserId = currentUserId;
                model.UserName = userName;
                model.CartItems = cartFallback;
                return View(model);
            }

            var cart = await _itemsClient.GetFromJsonAsync<List<CartItemViewModel>>("api/Cart")
                       ?? new List<CartItemViewModel>();

            if (!cart.Any())
            {
                TempData["ErrorMessage"] = "Varukorgen är tom.";
                return RedirectToAction("Index", "Cart");
            }

            foreach (var cartItem in cart)
            {
                var loanData = new
                {
                    itemId = cartItem.ItemId,
                    itemTitle = cartItem.Title,
                    userId = currentUserId,
                    quantity = cartItem.Quantity,
                    loanDate = model.LoanDate,
                    dueDate = model.DueDate,
                    returnedDate = (DateTime?)null,
                    status = "Pending"
                };

                var json = JsonSerializer.Serialize(loanData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _loansClient.PostAsync("api/Loans", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorText = await response.Content.ReadAsStringAsync();

                    ModelState.AddModelError(string.Empty,
                        $"Något gick fel när lånen skulle skapas. Status: {response.StatusCode}. Fel: {errorText}");

                    model.UserId = currentUserId;
                    model.UserName = userName;
                    model.CartItems = cart;
                    return View(model);
                }
            }

            foreach (var cartItem in cart)
            {
                await _itemsClient.DeleteAsync($"api/Cart/{cartItem.Id}");
            }

            TempData["SuccessMessage"] = "Lånen skapades!";
            return RedirectToAction("Index", "Profile");
        }
    }
}