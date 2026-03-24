using System.ComponentModel.DataAnnotations;

namespace LibraryAdminPanel.Models;

public class ReportObjectItem
{
    public int Id { get; set; }

    public int ReportId { get; set; }
    public Report? Report { get; set; }

    [Display(Name = "Objekt-id från extern databas")]
    public int CompletedObjectId { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    public string ObjectType { get; set; } = string.Empty;
    public string BorrowerName { get; set; } = string.Empty;
    public string BorrowerEmail { get; set; } = string.Empty;
}
