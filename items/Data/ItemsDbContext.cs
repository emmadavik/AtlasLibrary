using Microsoft.EntityFrameworkCore;
using items.Models;

namespace items.Data;

public class ItemsDbContext : DbContext
{
    public ItemsDbContext(DbContextOptions<ItemsDbContext> options)
        : base(options)
    {
    }
    public DbSet<Item> Items { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
}