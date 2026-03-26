using LibraryAdminPanel.Data;
using Microsoft.AspNetCore.Mvc;

namespace LibraryAdminPanel.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetDashboardData()
        {
            var result = new
            {
                reportCount = _context.Reports.Count(),
                reminderCount = _context.Reminders.Count()
            };

            return Ok(result);
        }
    }
}