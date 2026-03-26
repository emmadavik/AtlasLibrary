using System.ComponentModel.DataAnnotations;

namespace LibraryAdminPanel.Models;

public class ReminderFormViewModel
{
    public int Id { get; set; }

    [Display(Name = "Valt objekt")]
    public int CompletedObjectId { get; set; }

    [Required(ErrorMessage = "Påminnelse är obligatorisk.")]
    [Display(Name = "Påminnelse")]
    public string Message { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "Skickas datum")]
    public DateTime ReminderDate { get; set; } = DateTime.Today;

    public List<CompletedObject> CompletedObjects { get; set; } = new();
}