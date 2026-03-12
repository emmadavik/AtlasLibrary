using Microsoft.AspNetCore.Mvc;

namespace AtlasLibrary.Controllers
{
    public class ProfileController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ChangePassword()
        {
            return View();
        }
    }
}