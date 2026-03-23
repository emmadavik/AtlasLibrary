namespace items.Models;

public class Item
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}