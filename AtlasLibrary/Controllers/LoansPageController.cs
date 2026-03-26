using AtlasLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace AtlasLibrary.Controllers
{
    public class LoansPageController : Controller
    {
        
            private readonly HttpClient _httpClient;

            private readonly string _loansApiUrl = "https://atlas-loans-api-emmaa-bfdhc2h2h5bwh2a8.swedencentral-01.azurewebsites.net/api/Loans";
            private readonly string _itemsApiBaseUrl = "https://abdisalam-items-chauhsfzdabwdkg5.swedencentral-01.azurewebsites.net/";

            public LoansPageController(IHttpClientFactory httpClientFactory)
            {
                _httpClient = httpClientFactory.CreateClient();

            }



        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var cart = await _httpClient.GetFromJsonAsync<List<CartItemViewModel>>($"{_itemsApiBaseUrl}api/Cart")
                       ?? new List<CartItemViewModel>();

            if (!cart.Any())
            {
                TempData["ErrorMessage"] = "Varukorgen är tom.";
                return RedirectToAction("Index", "Cart");
            }

            var model = new CreateLoanViewModel
            {
                UserId = 1,
                UserName = "Testanvändare",
                LoanDate = DateTime.Today,
                DueDate = DateTime.Today.AddDays(14),
                CartItems = new List<CartItemViewModel>()
            };

            var allItems = await _httpClient.GetFromJsonAsync<List<ItemViewModel>>($"{_itemsApiBaseUrl}api/Items")
                           ?? new List<ItemViewModel>();

            foreach (var cartItem in cart)
            {
                var item = allItems.FirstOrDefault(i => i.Id == cartItem.ItemId);

                cartItem.Title = item?.Title ?? $"Bok #{cartItem.ItemId}";
                cartItem.ImageUrl = item?.ImageUrl ?? "";

                model.CartItems.Add(cartItem);
            }

            return View(model);
        }



        [HttpPost]
        public async Task<IActionResult> Create(CreateLoanViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var cartFallback = await _httpClient.GetFromJsonAsync<List<CartItemViewModel>>($"{_itemsApiBaseUrl}api/Cart")
                                   ?? new List<CartItemViewModel>();

                model.CartItems = cartFallback;
                return View(model);
            }

            var cart = await _httpClient.GetFromJsonAsync<List<CartItemViewModel>>($"{_itemsApiBaseUrl}api/Cart")
                       ?? new List<CartItemViewModel>();

            if (!cart.Any())
            {
                TempData["ErrorMessage"] = "Varukorgen är tom.";
                return RedirectToAction("Index", "Cart");
            }

            var allItems = await _httpClient.GetFromJsonAsync<List<ItemViewModel>>($"{_itemsApiBaseUrl}api/Items")
                           ?? new List<ItemViewModel>();

            foreach (var cartItem in cart)
            {
                var item = allItems.FirstOrDefault(i => i.Id == cartItem.ItemId);
                var itemTitle = item?.Title ?? $"Bok #{cartItem.ItemId}";

                var loanData = new
                {
                    itemId = cartItem.ItemId,
                    itemTitle = itemTitle,
                    userId = model.UserId,
                    quantity = cartItem.Quantity,
                    loanDate = DateTime.Today,
                    dueDate = model.DueDate,
                    returnedDate = (DateTime?)null,
                    status = "Pending"
                };

                var json = JsonSerializer.Serialize(loanData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_loansApiUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, "Något gick fel när lånen skulle skapas.");
                    model.CartItems = cart;
                    return View(model);
                }
            }

            foreach (var cartItem in cart)
            {
                await _httpClient.DeleteAsync($"{_itemsApiBaseUrl}api/Cart/{cartItem.Id}");
            }

            TempData["SuccessMessage"] = "Lånen skapades!";
            return RedirectToAction("Index", "Profile");
        }

    }
}