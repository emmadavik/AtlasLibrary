using Microsoft.AspNetCore.Mvc;

namespace AtlasLibrary.Controllers
{
    public class CartController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}