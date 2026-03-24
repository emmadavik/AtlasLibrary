using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using items.Data;
using items.Models;

namespace items.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly ItemsDbContext _context;

    public CartController(ItemsDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CartItem>>> GetCart()
    {
        var cartItems = await _context.CartItems.ToListAsync();
        return Ok(cartItems);
    }

    [HttpPost]
    public async Task<ActionResult<CartItem>> AddToCart(CartItem item)
    {
        var existing = await _context.CartItems
            .FirstOrDefaultAsync(c => c.ItemId == item.ItemId);

        if (existing != null)
        {
            existing.Quantity += item.Quantity;
            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        _context.CartItems.Add(item);
        await _context.SaveChangesAsync();

        return Ok(item);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateQuantity(int id, CartItem updated)
    {
        var item = await _context.CartItems.FindAsync(id);

        if (item == null)
            return NotFound();

        item.Quantity = updated.Quantity;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Remove(int id)
    {
        var item = await _context.CartItems.FindAsync(id);

        if (item == null)
            return NotFound();

        _context.CartItems.Remove(item);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}