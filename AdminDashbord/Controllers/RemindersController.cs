using AdminDashbord.Data;
using AdminDashbord.Models;
using AdminDashbord.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminDashbord.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RemindersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ExternalObjectApiService _externalObjectApiService;

    public RemindersController(ApplicationDbContext context, ExternalObjectApiService externalObjectApiService)
    {
        _context = context;
        _externalObjectApiService = externalObjectApiService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Reminder>>> GetReminders()
    {
        var reminders = await _context.Reminders.ToListAsync();
        return Ok(reminders);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Reminder>> GetReminder(int id)
    {
        var reminder = await _context.Reminders.FindAsync(id);

        if (reminder == null)
        {
            return NotFound();
        }

        return Ok(reminder);
    }

    [HttpPost]
    public async Task<ActionResult<Reminder>> CreateReminder([FromBody] Reminder reminder)
    {
        _context.Reminders.Add(reminder);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetReminder), new { id = reminder.Id }, reminder);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateReminder(int id, [FromBody] Reminder updatedReminder)
    {
        var existingReminder = await _context.Reminders.FindAsync(id);

        if (existingReminder == null)
        {
            return NotFound();
        }

        existingReminder.CompletedObjectId = updatedReminder.CompletedObjectId;
        existingReminder.BorrowerName = updatedReminder.BorrowerName;
        existingReminder.BorrowerEmail = updatedReminder.BorrowerEmail;
        existingReminder.ObjectTitle = updatedReminder.ObjectTitle;
        existingReminder.Message = updatedReminder.Message;
        existingReminder.ReminderDate = updatedReminder.ReminderDate;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReminder(int id)
    {
        var reminder = await _context.Reminders.FindAsync(id);

        if (reminder == null)
        {
            return NotFound();
        }

        _context.Reminders.Remove(reminder);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("completed-objects")]
    public async Task<ActionResult<IEnumerable<CompletedObject>>> GetCompletedObjects()
    {
        var completedObjects = await _externalObjectApiService.GetCompletedObjectsAsync();
        return Ok(completedObjects);
    }

    [HttpGet("completed-objects/{id}")]
    public async Task<ActionResult<CompletedObject>> GetCompletedObjectById(int id)
    {
        var completedObjects = await _externalObjectApiService.GetCompletedObjectsAsync();
        var completedObject = completedObjects.FirstOrDefault(x => x.Id == id);

        if (completedObject == null)
        {
            return NotFound();
        }

        return Ok(completedObject);
    }
}