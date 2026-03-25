using LibraryAdminPanel.Data;
using LibraryAdminPanel.Models;
using LibraryAdminPanel.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryAdminPanel.Controllers;

public class ReportsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ExternalObjectApiService _externalObjectApiService;

    public ReportsController(ApplicationDbContext context, ExternalObjectApiService externalObjectApiService)
    {
        _context = context;
        _externalObjectApiService = externalObjectApiService;
    }

    public async Task<IActionResult> Index()
    {
        var reports = await _context.Reports
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return View(reports);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var viewModel = new ReportFormViewModel
        {
            CompletedObjects = await _externalObjectApiService.GetCompletedObjectsAsync()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReportFormViewModel model)
    {
        model.CompletedObjects = await _externalObjectApiService.GetCompletedObjectsAsync();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var report = new Report
        {
            Title = model.Title,
            Summary = model.Summary,
            CreatedAt = DateTime.UtcNow,
            SelectedObjectIds = model.SelectedObjectIds != null && model.SelectedObjectIds.Any()
                ? string.Join(",", model.SelectedObjectIds)
                : null
        };

        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var report = await _context.Reports.FirstOrDefaultAsync(r => r.Id == id);
        if (report == null)
        {
            return NotFound();
        }

        var selectedIds = new List<int>();

        if (!string.IsNullOrWhiteSpace(report.SelectedObjectIds))
        {
            selectedIds = report.SelectedObjectIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(idText => int.TryParse(idText, out var parsedId) ? parsedId : 0)
                .Where(parsedId => parsedId != 0)
                .ToList();
        }

        var viewModel = new ReportFormViewModel
        {
            Id = report.Id,
            Title = report.Title,
            Summary = report.Summary,
            SelectedObjectIds = selectedIds,
            CompletedObjects = await _externalObjectApiService.GetCompletedObjectsAsync()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ReportFormViewModel model)
    {
        model.CompletedObjects = await _externalObjectApiService.GetCompletedObjectsAsync();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var report = await _context.Reports.FirstOrDefaultAsync(r => r.Id == model.Id);
        if (report == null)
        {
            return NotFound();
        }

        report.Title = model.Title;
        report.Summary = model.Summary;
        report.SelectedObjectIds = model.SelectedObjectIds != null && model.SelectedObjectIds.Any()
            ? string.Join(",", model.SelectedObjectIds)
            : null;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var report = await _context.Reports.FirstOrDefaultAsync(r => r.Id == id);
        if (report == null)
        {
            return NotFound();
        }

        return View(report);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var report = await _context.Reports.FirstOrDefaultAsync(r => r.Id == id);
        if (report != null)
        {
            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}