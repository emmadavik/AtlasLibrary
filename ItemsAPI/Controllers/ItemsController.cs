using ItemsAPI.Data;
using ItemsAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace ItemsAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ItemsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_context.Items.ToList());
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var item = _context.Items.Find(id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpPost]
    public IActionResult Create(Item item)
    {
        if (item.Type == "Utrustning")
        {
            item.Author = string.Empty;
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _context.Items.Add(item);
        _context.SaveChanges();
        return Ok(item);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, Item updatedItem)
    {
        var item = _context.Items.Find(id);
        if (item == null) return NotFound();

        if (item.Type == "Utrustning")
        {
            item.Author = string.Empty;
        }

        item.Title = updatedItem.Title;
        item.Author = updatedItem.Author;
        item.Type = updatedItem.Type;
        item.Description = updatedItem.Description;
        item.IsAvailable = updatedItem.IsAvailable;
        item.ImageUrl = updatedItem.ImageUrl;

        _context.SaveChanges();
        return Ok(item);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var item = _context.Items.Find(id);
        if (item == null) return NotFound();

        _context.Items.Remove(item);
        _context.SaveChanges();
        return Ok();
    }
}