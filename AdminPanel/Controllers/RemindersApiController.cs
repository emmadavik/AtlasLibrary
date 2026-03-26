using LibraryAdminPanel.Data;
using LibraryAdminPanel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryAdminPanel.Controllers
{
    [ApiController]
    [Route("api/reminders")]
    public class RemindersApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RemindersApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetReminders()
        {
            var reminders = await _context.Reminders
                .OrderByDescending(r => r.ReminderDate)
                .Select(r => new
                {
                    r.Id,
                    r.CompletedObjectId,
                    r.BorrowerName,
                    r.BorrowerEmail,
                    r.ObjectTitle,
                    r.Message,
                    r.ReminderDate
                })
                .ToListAsync();

            return Ok(reminders);
        }

        [HttpPost]
        public async Task<IActionResult> CreateReminder([FromBody] Reminder reminder)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Reminders.Add(reminder);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Påminnelse skapad",
                reminder.Id
            });
        }
    }
}