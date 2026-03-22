using AtlasLibrary.LoansApi.Data;
using AtlasLibrary.LoansApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Net.Http;
using System.Text.Json;

namespace AtlasLibrary.LoansApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoansController : ControllerBase
    {
        private readonly LoansDbContext _context;

        public LoansController(LoansDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Loan>>> GetLoans()
        {
            return await _context.Loans.ToListAsync();
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<Loan>> GetLoan(int id)
        {
            var loan = await _context.Loans.FindAsync(id);

            if (loan == null)
            {
                return NotFound();
            }

            return loan;
        }

        [HttpPost]
        public async Task<ActionResult<Loan>> CreateLoan(Loan loan)
        {
            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetLoan), new { id = loan.Id }, loan);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLoan(int id, Loan updatedLoan)
        {
            if (id != updatedLoan.Id)
            {
                return BadRequest();
            }

            _context.Entry(updatedLoan).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Loans.Any(l => l.Id == id))
                {
                    return NotFound();
                }

                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLoan(int id)
        {
            var loan = await _context.Loans.FindAsync(id);

            if (loan == null)
            {
                return NotFound();
            }

            _context.Loans.Remove(loan);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}/return")]
        public async Task<IActionResult> ReturnLoan(int id)
        {
            var loan = await _context.Loans.FindAsync(id);

            if (loan == null)
            {
                return NotFound();
            }

            loan.Status = "Returned";
            loan.ReturnedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Loan>>> GetLoansByUser(int userId)
        {
            var loans = await _context.Loans
                .Where(l => l.UserId == userId)
                .ToListAsync();

            return loans;
        }

        [HttpPut("{id}/extend")]
        public async Task<IActionResult> ExtendLoan(int id, [FromBody] DateTime newDueDate)
        {
            var loan = await _context.Loans.FindAsync(id);

            if (loan == null)
            {
                return NotFound();
            }

            if (loan.Status == "Returned")
            {
                return BadRequest("Det går inte att förlänga ett återlämnat lån.");
            }

            if (newDueDate <= loan.DueDate)
            {
                return BadRequest("Nytt datum måste vara senare än nuvarande förfallodatum.");
            }

            loan.DueDate = newDueDate;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("report")]
        public IActionResult GetReport()
        {
            var report = new LoanReportDto
            {
                TotalLoans = _context.Loans.Count(),
                ActiveLoans = _context.Loans.Count(l => l.ReturnedDate == null),
                ReturnedLoans = _context.Loans.Count(l => l.ReturnedDate != null),
                OverdueLoans = _context.Loans.Count(l => l.ReturnedDate == null && l.DueDate < DateTime.Today),
                DueSoonLoans = _context.Loans.Count(l =>
                    l.ReturnedDate == null &&
                    l.DueDate >= DateTime.Today &&
                    l.DueDate <= DateTime.Today.AddDays(3))
            };

            return Ok(report);
        }
    }
}