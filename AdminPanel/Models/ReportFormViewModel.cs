using System.ComponentModel.DataAnnotations;

namespace LibraryAdminPanel.Models;

public class ReportFormViewModel
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Rapportnamn")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "Välj färdiga objekt")]
    public List<int> SelectedObjectIds { get; set; } = new();

    [Display(Name = "Sammanfattning")]
    public string Summary { get; set; } = string.Empty;

    public List<CompletedObject> CompletedObjects { get; set; } = new();
}
