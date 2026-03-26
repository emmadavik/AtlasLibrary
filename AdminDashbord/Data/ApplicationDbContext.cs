using AdminDashbord.Models;
using Microsoft.EntityFrameworkCore;

namespace AdminDashbord.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Report> Reports { get; set; }
    public DbSet<Reminder> Reminders { get; set; }
}