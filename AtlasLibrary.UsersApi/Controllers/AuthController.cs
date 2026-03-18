using AtlasLibrary.UsersApi.Data;
using AtlasLibrary.UsersApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AtlasLibrary.UsersApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UsersDbContext _db;
    private readonly IConfiguration _cfg;
    private readonly PasswordHasher<Anvandare> _hasher = new();

    public AuthController(UsersDbContext db, IConfiguration cfg)
    {
        _db = db;
        _cfg = cfg;
    }

    public record LoginRequest(string Epost, string Losenord);
    public record RegisterRequest(string Namn, string Epost, string Losenord);

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest req)
    {
        var user = await _db.Anvandare.FirstOrDefaultAsync(u => u.Epost == req.Epost);
        if (user is null) return Unauthorized();

        var ok = _hasher.VerifyHashedPassword(user, user.Losenord, req.Losenord) != PasswordVerificationResult.Failed;
        if (!ok) return Unauthorized();

        var key = _cfg["Jwt:Key"]!;
        var token = new JwtSecurityToken(
            issuer: _cfg["Jwt:Issuer"],
            audience: _cfg["Jwt:Audience"],
            claims: new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Roll)
            },
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256)
        );

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token),
            roll = user.Roll
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Namn) ||
            string.IsNullOrWhiteSpace(req.Epost) ||
            string.IsNullOrWhiteSpace(req.Losenord))
        {
            return BadRequest("Alla fält måste fyllas i.");
        }

        var finnsRedan = await _db.Anvandare.AnyAsync(u => u.Epost == req.Epost);
        if (finnsRedan)
        {
            return BadRequest("E-postadressen används redan.");
        }

        var user = new Anvandare
        {
            Namn = req.Namn,
            Epost = req.Epost,
            Roll = "User"
        };

        user.Losenord = _hasher.HashPassword(user, req.Losenord);

        _db.Anvandare.Add(user);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Konto skapat"
        });
    }
}