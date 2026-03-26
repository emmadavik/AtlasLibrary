namespace AdminDashbord.Models;

public class Reminder
{
    public int Id { get; set; }

    public int CompletedObjectId { get; set; }

    public string BorrowerName { get; set; } = string.Empty;

    public string BorrowerEmail { get; set; } = string.Empty;

    public string ObjectTitle { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public DateTime ReminderDate { get; set; } = DateTime.Today;
}