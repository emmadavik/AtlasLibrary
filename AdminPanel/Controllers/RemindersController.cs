using LibraryAdminPanel.Data;
using LibraryAdminPanel.Models;
using LibraryAdminPanel.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryAdminPanel.Controllers;

public class RemindersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ExternalObjectApiService _externalObjectApiService;

    public RemindersController(ApplicationDbContext context, ExternalObjectApiService externalObjectApiService)
    {
        _context = context;
        _externalObjectApiService = externalObjectApiService;
    }

    public async Task<IActionResult> Index()
    {
        var reminders = await _context.Reminders
            .OrderByDescending(reminder => reminder.ReminderDate)
            .ToListAsync();

        return View(reminders);
    }

    public async Task<IActionResult> Create()
    {
        var viewModel = new ReminderFormViewModel
        {
            CompletedObjects = await _externalObjectApiService.GetCompletedObjectsAsync()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReminderFormViewModel viewModel)
    {
        viewModel.CompletedObjects = await _externalObjectApiService.GetCompletedObjectsAsync();

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var selectedObject = viewModel.CompletedObjects.FirstOrDefault(item => item.Id == viewModel.CompletedObjectId);
        if (selectedObject == null)
        {
            ModelState.AddModelError(string.Empty, "Det valda objektet kunde inte hämtas från API:t.");
            return View(viewModel);
        }

        var reminder = new Reminder
        {
            CompletedObjectId = selectedObject.Id,
            BorrowerName = selectedObject.BorrowerName,
            BorrowerEmail = selectedObject.BorrowerEmail,
            ObjectTitle = selectedObject.Title,
            Message = viewModel.Message,
            ReminderDate = viewModel.ReminderDate
        };

        _context.Reminders.Add(reminder);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var reminder = await _context.Reminders.FirstOrDefaultAsync(currentReminder => currentReminder.Id == id);
        if (reminder == null)
        {
            return NotFound();
        }

        var viewModel = new ReminderFormViewModel
        {
            Id = reminder.Id,
            CompletedObjectId = reminder.CompletedObjectId,
            Message = reminder.Message,
            ReminderDate = reminder.ReminderDate,
            CompletedObjects = await _externalObjectApiService.GetCompletedObjectsAsync()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ReminderFormViewModel viewModel)
    {
        viewModel.CompletedObjects = await _externalObjectApiService.GetCompletedObjectsAsync();

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var reminder = await _context.Reminders.FirstOrDefaultAsync(currentReminder => currentReminder.Id == viewModel.Id);
        if (reminder == null)
        {
            return NotFound();
        }

        var selectedObject = viewModel.CompletedObjects.FirstOrDefault(item => item.Id == viewModel.CompletedObjectId);
        if (selectedObject == null)
        {
            ModelState.AddModelError(string.Empty, "Det valda objektet kunde inte hämtas från API:t.");
            return View(viewModel);
        }

        reminder.CompletedObjectId = selectedObject.Id;
        reminder.BorrowerName = selectedObject.BorrowerName;
        reminder.BorrowerEmail = selectedObject.BorrowerEmail;
        reminder.ObjectTitle = selectedObject.Title;
        reminder.Message = viewModel.Message;
        reminder.ReminderDate = viewModel.ReminderDate;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var reminder = await _context.Reminders.FirstOrDefaultAsync(currentReminder => currentReminder.Id == id);
        if (reminder == null)
        {
            return NotFound();
        }

        return View(reminder);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var reminder = await _context.Reminders.FirstOrDefaultAsync(currentReminder => currentReminder.Id == id);
        if (reminder != null)
        {
            _context.Reminders.Remove(reminder);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
