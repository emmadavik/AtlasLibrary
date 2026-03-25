using System.ComponentModel.DataAnnotations;

namespace LibraryAdminPanel.Models;

public class Report
{
    public int Id { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string SelectedObjectIds { get; set; } = string.Empty;
}