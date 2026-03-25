using AtlasLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace AtlasLibrary.Controllers
{
    public class LoansPageController : Controller
    {
        //private readonly HttpClient _httpClient;
        //private readonly HttpClient _loansClient;
        //private readonly HttpClient _itemsClient;
        private readonly HttpClient _loansClient;
        private readonly HttpClient _ItemsClient;
        private readonly HttpClient _equipmentItemsClient;

        public LoansPageController(IHttpClientFactory factory)
        {
            //_loansClient = new HttpClient();
            //_loansClient.BaseAddress = new Uri("https://localhost:7024/");

            //_itemsClient = factory.CreateClient("itemsApi");

            _loansClient = new HttpClient();
            _loansClient.BaseAddress = new Uri("https://localhost:7024/");

            _ItemsClient = factory.CreateClient("itemsApi");
            _equipmentItemsClient = factory.CreateClient("equipmentItemsApi");

        }

        [HttpGet]
        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var cart = await _ItemsClient.GetFromJsonAsync<List<CartItemViewModel>>("api/Cart")
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

            foreach (var cartItem in cart)
            {
                
                var itemResponse = await _ItemsClient.GetAsync($"api/Items/{cartItem.ItemId}");

                if (itemResponse.IsSuccessStatusCode)
                {
                    var json = await itemResponse.Content.ReadAsStringAsync();

                    var item = JsonSerializer.Deserialize<ItemViewModel>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                   
                    cartItem.Title = item?.Title ?? $"Bok #{cartItem.ItemId}";
                    cartItem.ImageUrl = item?.ImageUrl ?? "";
                }

                model.CartItems.Add(cartItem);
            }

            return View(model);
        }



        [HttpPost]
        public async Task<IActionResult> Create(CreateLoanViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var cartFallback = await _ItemsClient.GetFromJsonAsync<List<CartItemViewModel>>("api/Cart")
                                   ?? new List<CartItemViewModel>();

                model.CartItems = cartFallback;
                return View(model);
            }

            var cart = await _ItemsClient.GetFromJsonAsync<List<CartItemViewModel>>("api/Cart")
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
                    userId = model.UserId,
                    quantity = cartItem.Quantity,
                    loanDate = DateTime.Today,
                    dueDate = model.DueDate,
                    returnedDate = (DateTime?)null,
                    status = "Pending"
                };

                var json = JsonSerializer.Serialize(loanData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _loansClient.PostAsync("api/Loans", content);

                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, "Något gick fel när lånen skulle skapas.");
                    model.CartItems = cart;
                    return View(model);
                }
            }

            foreach (var cartItem in cart)
            {
                await _ItemsClient.DeleteAsync($"api/Cart/{cartItem.Id}");
            }

            TempData["SuccessMessage"] = "Lånen skapades!";
            return RedirectToAction("Index", "Profile");
        }
        [HttpPost]
        public async Task<IActionResult> RequestReturn(int id, int userId = 1)
        {
            var response = await _loansClient.PutAsync($"api/Loans/{id}/request-return", null);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Återlämning markerad och väntar på bekräftelse.";
            }
            else
            {
                TempData["ErrorMessage"] = "Det gick inte att markera återlämning.";
            }

            return RedirectToAction("Index", "Profile");
        }

    }
}