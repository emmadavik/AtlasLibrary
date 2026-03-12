using AtlasLibrary.UsersApi.Data;
using AtlasLibrary.UsersApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AtlasLibrary.UsersApi.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly UsersDbContext _db;
    private readonly PasswordHasher<Anvandare> _hasher = new();

    public UsersController(UsersDbContext db)
    {
        _db = db;
    }

    // =========================
    // READ (R i CRUD)
    // =========================

    // ADMIN: Se alla användare
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var users = await _db.Anvandare
            .Select(u => new { u.Id, u.Namn, u.Epost, u.Roll })
            .ToListAsync();

        return Ok(users);
    }

    // ADMIN: Hämta valfri användare via id
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Get(int id)
    {
        var user = await _db.Anvandare.FindAsync(id);
        if (user is null) return NotFound();

        return Ok(new { user.Id, user.Namn, user.Epost, user.Roll });
    }

    // USER/ADMIN: Hämta sin egen profil
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idClaim, out var userId)) return Unauthorized();

        var user = await _db.Anvandare.FindAsync(userId);
        if (user is null) return NotFound();

        return Ok(new { user.Id, user.Namn, user.Epost, user.Roll });
    }

    // =========================
    // CREATE (C i CRUD)
    // =========================

    // REGISTRERING: Öppen, men alla blir alltid "User"
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create(Anvandare user)
    {
        user.Roll = "User";
        user.Losenord = _hasher.HashPassword(user, user.Losenord);

        _db.Anvandare.Add(user);
        await _db.SaveChangesAsync();

        return Created($"/api/users/{user.Id}",
            new { user.Id, user.Namn, user.Epost, user.Roll });
    }

    // =========================
    // UPDATE (U i CRUD)
    // =========================

    // ADMIN: Uppdatera valfri användare via id (men inte roll här – vi håller det enkelt)
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, Anvandare updated)
    {
        var existing = await _db.Anvandare.FindAsync(id);
        if (existing is null) return NotFound();

        existing.Namn = updated.Namn;
        existing.Epost = updated.Epost;

        if (!string.IsNullOrWhiteSpace(updated.Losenord))
            existing.Losenord = _hasher.HashPassword(existing, updated.Losenord);

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // USER/ADMIN: Uppdatera sin egen profil
    [HttpPut("me")]
    [Authorize]
    public async Task<IActionResult> UpdateMe(Anvandare updated)
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idClaim, out var userId)) return Unauthorized();

        var existing = await _db.Anvandare.FindAsync(userId);
        if (existing is null) return NotFound();

        existing.Namn = updated.Namn;
        existing.Epost = updated.Epost;

        if (!string.IsNullOrWhiteSpace(updated.Losenord))
            existing.Losenord = _hasher.HashPassword(existing, updated.Losenord);

        // OBS: Roll ändras inte här
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // =========================
    // DELETE (D i CRUD)
    // =========================

    // ADMIN: Ta bort valfri användare
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _db.Anvandare.FindAsync(id);
        if (existing is null) return NotFound();

        _db.Anvandare.Remove(existing);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}