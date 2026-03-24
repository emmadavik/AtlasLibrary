using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryAdminPanel.Models;

public class Report
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Rapportnamn")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "Valda objekt")]
    public List<ReportObjectItem> SelectedObjects { get; set; } = new();

    [NotMapped]
    [Display(Name = "Antal objekt")]
    public int ObjectCount => SelectedObjects.Count;

    [Display(Name = "Sammanfattning")]
    public string Summary { get; set; } = string.Empty;

    [Display(Name = "Skapad datum")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
