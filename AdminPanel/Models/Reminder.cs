using System.ComponentModel.DataAnnotations;

namespace LibraryAdminPanel.Models;

public class Reminder
{
    public int Id { get; set; }

    [Display(Name = "Valt objekt")]
    public int CompletedObjectId { get; set; }

    [Required]
    [Display(Name = "Låntagare")]
    public string BorrowerName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "E-post")]
    public string BorrowerEmail { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Objektets titel")]
    public string ObjectTitle { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Påminnelse")]
    public string Message { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "Skickas datum")]
    public DateTime ReminderDate { get; set; } = DateTime.Today;
}