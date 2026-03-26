using AdminDashbord.Data;
using AdminDashbord.Models;
using AdminDashbord.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminDashbord.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ExternalObjectApiService _externalObjectApiService;

    public ReportsController(ApplicationDbContext context, ExternalObjectApiService externalObjectApiService)
    {
        _context = context;
        _externalObjectApiService = externalObjectApiService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Report>>> GetReports()
    {
        var reports = await _context.Reports.ToListAsync();
        return Ok(reports);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Report>> GetReport(int id)
    {
        var report = await _context.Reports.FindAsync(id);

        if (report == null)
        {
            return NotFound();
        }

        return Ok(report);
    }

    [HttpPost]
    public async Task<ActionResult<Report>> CreateReport([FromBody] Report report)
    {
        report.CreatedAt = DateTime.UtcNow;

        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetReport), new { id = report.Id }, report);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateReport(int id, [FromBody] Report updatedReport)
    {
        var existingReport = await _context.Reports.FindAsync(id);

        if (existingReport == null)
        {
            return NotFound();
        }

        existingReport.Title = updatedReport.Title;
        existingReport.Summary = updatedReport.Summary;
        existingReport.SelectedObjectIds = updatedReport.SelectedObjectIds;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReport(int id)
    {
        var report = await _context.Reports.FindAsync(id);

        if (report == null)
        {
            return NotFound();
        }

        _context.Reports.Remove(report);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("completed-objects")]
    public async Task<ActionResult<IEnumerable<CompletedObject>>> GetCompletedObjects()
    {
        var completedObjects = await _externalObjectApiService.GetCompletedObjectsAsync();
        return Ok(completedObjects);
    }
}