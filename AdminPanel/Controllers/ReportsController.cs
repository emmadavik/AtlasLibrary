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
            .Include(report => report.SelectedObjects)
            .OrderByDescending(report => report.CreatedAt)
            .ToListAsync();

        return View(reports);
    }

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
    public async Task<IActionResult> Create(ReportFormViewModel viewModel)
    {
        viewModel.CompletedObjects = await _externalObjectApiService.GetCompletedObjectsAsync();

        if (!viewModel.SelectedObjectIds.Any())
        {
            ModelState.AddModelError(string.Empty, "Välj minst ett objekt till rapporten.");
        }

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var selectedObjects = CreateReportItems(viewModel.CompletedObjects, viewModel.SelectedObjectIds);

        if (!selectedObjects.Any())
        {
            ModelState.AddModelError(string.Empty, "De valda objekten kunde inte hämtas från API:t.");
            return View(viewModel);
        }

        var report = new Report
        {
            Title = viewModel.Title,
            Summary = viewModel.Summary,
            CreatedAt = DateTime.Now,
            SelectedObjects = selectedObjects
        };

        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var report = await _context.Reports
            .Include(currentReport => currentReport.SelectedObjects)
            .FirstOrDefaultAsync(currentReport => currentReport.Id == id);

        if (report == null)
        {
            return NotFound();
        }

        var viewModel = new ReportFormViewModel
        {
            Id = report.Id,
            Title = report.Title,
            Summary = report.Summary,
            SelectedObjectIds = report.SelectedObjects.Select(item => item.CompletedObjectId).ToList(),
            CompletedObjects = await _externalObjectApiService.GetCompletedObjectsAsync()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ReportFormViewModel viewModel)
    {
        viewModel.CompletedObjects = await _externalObjectApiService.GetCompletedObjectsAsync();

        if (!viewModel.SelectedObjectIds.Any())
        {
            ModelState.AddModelError(string.Empty, "Välj minst ett objekt till rapporten.");
        }

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var report = await _context.Reports
            .Include(currentReport => currentReport.SelectedObjects)
            .FirstOrDefaultAsync(currentReport => currentReport.Id == viewModel.Id);

        if (report == null)
        {
            return NotFound();
        }

        var newItems = CreateReportItems(viewModel.CompletedObjects, viewModel.SelectedObjectIds);

        if (!newItems.Any())
        {
            ModelState.AddModelError(string.Empty, "De valda objekten kunde inte hämtas från API:t.");
            return View(viewModel);
        }

        report.Title = viewModel.Title;
        report.Summary = viewModel.Summary;

        _context.ReportObjectItems.RemoveRange(report.SelectedObjects);
        report.SelectedObjects = newItems;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var report = await _context.Reports
            .Include(currentReport => currentReport.SelectedObjects)
            .FirstOrDefaultAsync(currentReport => currentReport.Id == id);

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
        var report = await _context.Reports
            .Include(currentReport => currentReport.SelectedObjects)
            .FirstOrDefaultAsync(currentReport => currentReport.Id == id);

        if (report != null)
        {
            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private static List<ReportObjectItem> CreateReportItems(List<CompletedObject> completedObjects, List<int> selectedObjectIds)
    {
        return completedObjects
            .Where(item => selectedObjectIds.Contains(item.Id))
            .Select(item => new ReportObjectItem
            {
                CompletedObjectId = item.Id,
                Title = item.Title,
                ObjectType = item.ObjectType,
                BorrowerName = item.BorrowerName,
                BorrowerEmail = item.BorrowerEmail
            })
            .ToList();
    }
}
