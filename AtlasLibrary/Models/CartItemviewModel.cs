namespace AtlasLibrary.Models;


public class CartItemViewModel
{
    public int Id { get; set; }


    public int ItemId { get; set; }


    public string Title { get; set; } = "";


    public string Author { get; set; } = "";


    public string Type { get; set; } = "";
    public string Description { get; set; } = "";


    public string ImageUrl { get; set; } = "";


    public int Quantity { get; set; }
}