using AtlasLibrary.LoansApi.Data;
using AtlasLibrary.LoansApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Net.Http.Headers;

namespace AtlasLibrary.LoansApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoansController : ControllerBase
    {
        private readonly LoansDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public LoansController(LoansDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
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

        [HttpPut("{id}/request-return")]
        public async Task<IActionResult> RequestReturn(int id)
        {
            var loan = await _context.Loans.FindAsync(id);

            if (loan == null)
            {
                return NotFound();
            }

            if (loan.Status == "Returned")
            {
                return BadRequest("Lånet är redan återlämnat.");
            }

            loan.Status = "ReturnRequested";
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}/confirm-return")]
        public async Task<IActionResult> ConfirmReturn(int id)
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

        [HttpGet("admin-report-items")]
        public async Task<ActionResult<IEnumerable<AdminLoanReportItemDto>>> GetAdminReportItems()
        {
            var users = new List<UserDto>();

            try
            {
                var usersClient = _httpClientFactory.CreateClient("UsersApi");

                var incomingAuthHeader = Request.Headers["Authorization"].ToString();

                if (!string.IsNullOrWhiteSpace(incomingAuthHeader))
                {
                    usersClient.DefaultRequestHeaders.Authorization =
                        AuthenticationHeaderValue.Parse(incomingAuthHeader);
                }

                var response = await usersClient.GetAsync("api/users");

                if (response.IsSuccessStatusCode)
                {
                    var usersJson = await response.Content.ReadAsStringAsync();

                    users = JsonSerializer.Deserialize<List<UserDto>>(usersJson,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }) ?? new List<UserDto>();
                }
            }
            catch
            {
                // fallback längre ner
            }

            var result = _context.Loans
                .AsEnumerable()
                .Select(l =>
                {
                    var user = users.FirstOrDefault(u => u.Id == l.UserId);

                    return new AdminLoanReportItemDto
                    {
                        Id = l.Id,
                        Title = $"Bok #{l.ItemId}",
                        ObjectType = "Book",
                        BorrowerName = user?.Namn ?? $"User {l.UserId}",
                        BorrowerEmail = user?.Epost ?? $"user{l.UserId}@atlaslibrary.se",
                        BorrowedDate = l.LoanDate,
                        ReturnedDate = l.ReturnedDate,
                        Status = l.Status,
                        Quantity = l.Quantity
                    };
                })
                .ToList();

            return Ok(result);
        }

        private class UserDto
        {
            public int Id { get; set; }
            public string Namn { get; set; } = string.Empty;
            public string Epost { get; set; } = string.Empty;
        }
    }
}