using LibraryAdminPanel.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryAdminPanel.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Report> Reports => Set<Report>();
    public DbSet<ReportObjectItem> ReportObjectItems => Set<ReportObjectItem>();
    public DbSet<Reminder> Reminders => Set<Reminder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Report>()
            .HasMany(x => x.SelectedObjects)
            .WithOne(x => x.Report)
            .HasForeignKey(x => x.ReportId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
