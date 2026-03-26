namespace AdminDashbord.Models;

public class Report
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Summary { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string SelectedObjectIds { get; set; } = string.Empty;
}