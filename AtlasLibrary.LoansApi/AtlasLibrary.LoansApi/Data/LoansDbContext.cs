using AtlasLibrary.LoansApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AtlasLibrary.LoansApi.Data
{
    public class LoansDbContext : DbContext
    {
        public LoansDbContext(DbContextOptions<LoansDbContext> options)
            : base(options)
        {
        }

        public DbSet<Loan> Loans { get; set; }
    }
}
