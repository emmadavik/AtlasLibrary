using LibraryAdminPanel.Data;
using Microsoft.AspNetCore.Mvc;

namespace LibraryAdminPanel.Controllers;

public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        ViewBag.ReportCount = _context.Reports.Count();
        ViewBag.ReminderCount = _context.Reminders.Count();
        return View();
    }
}