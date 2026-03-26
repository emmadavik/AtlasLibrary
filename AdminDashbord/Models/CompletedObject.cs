namespace AdminDashbord.Models;

public class CompletedObject
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string ObjectType { get; set; } = string.Empty;

    public string BorrowerName { get; set; } = string.Empty;

    public string BorrowerEmail { get; set; } = string.Empty;

    public DateTime BorrowedDate { get; set; }

    public DateTime? ReturnedDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public int Quantity { get; set; }
}