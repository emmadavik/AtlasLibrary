namespace items.Models;

public class CartItem
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public int Quantity { get; set; }
}