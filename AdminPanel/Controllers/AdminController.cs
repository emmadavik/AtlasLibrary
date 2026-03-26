using LibraryAdminPanel.Data;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace LibraryAdminPanel.Controllers;

public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;

    public AdminController(ApplicationDbContext context, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IActionResult> Index()
    {
        // Hans gamla funktionalitet (lämna orörd)
        ViewBag.ReportCount = _context.Reports.Count();
        ViewBag.ReminderCount = _context.Reminders.Count();

        try
        {
            var token = HttpContext.Session.GetString("JwtToken");

            if (!string.IsNullOrEmpty(token))
            {
                var client = _httpClientFactory.CreateClient("UsersApi");

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync("api/users/me");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    ViewBag.UserInfo = json;
                }
                else
                {
                    ViewBag.UserInfo = "Kunde inte hämta användare";
                }
            }
            else
            {
                ViewBag.UserInfo = "Ingen användare inloggad";
            }
        }
        catch
        {
            ViewBag.UserInfo = "API-anrop misslyckades";
        }

        return View();
    }
}