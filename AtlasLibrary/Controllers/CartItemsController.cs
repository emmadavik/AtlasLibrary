using AtlasLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
namespace AtlasLibrary.Controllers;


public class CartController : Controller
{
   private readonly HttpClient _httpClient;


   public CartController(IHttpClientFactory factory)
   {
       _httpClient = factory.CreateClient("ItemsService");
   }


   public async Task<IActionResult> Index()
   {
       var cart = await _httpClient.GetFromJsonAsync<List<CartItemViewModel>>(
           "api/Cart"
       ) ?? new List<CartItemViewModel>();


       return View(cart);
   }
   [HttpPost]
   public async Task<IActionResult> Add(int itemId)
   {
       var item = new
       {
           ItemId = itemId,
           Quantity = 1
       };


       await _httpClient.PostAsJsonAsync("api/Cart", item);


       return RedirectToAction("Index");
   }
   [HttpPost]
   public async Task<IActionResult> IncreaseQuantity(int id)
   {
       var cart = await _httpClient.GetFromJsonAsync<List<CartItemViewModel>>("api/Cart")
                  ?? new List<CartItemViewModel>();


       var item = cart.FirstOrDefault(x => x.Id == id);


       if (item != null)
       {
           item.Quantity++;


           await _httpClient.PutAsJsonAsync($"api/Cart/{id}", item);
       }


       return RedirectToAction("Index");
   }
  
  
   [HttpPost]
   public async Task<IActionResult> DecreaseQuantity(int id)
   {
       var cart = await _httpClient.GetFromJsonAsync<List<CartItemViewModel>>("api/Cart")
                  ?? new List<CartItemViewModel>();


       var item = cart.FirstOrDefault(x => x.Id == id);


       if (item != null)
       {
           if (item.Quantity > 1)
           {
               item.Quantity--;


               await _httpClient.PutAsJsonAsync($"api/Cart/{id}", item);
           }
           else
           {
               await _httpClient.DeleteAsync($"api/Cart/{id}");
           }
       }


       return RedirectToAction("Index");
   }
   [HttpPost]
   public async Task<IActionResult> Remove(int id)
   {
       await _httpClient.DeleteAsync($"api/Cart/{id}");
       return RedirectToAction("Index");
   }
}
