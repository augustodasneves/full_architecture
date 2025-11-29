using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.DTOs;
using UserAccountApi.Data;
using UserAccountApi.Domain;

namespace UserAccountApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _context;

    public UserController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("me/{phoneNumber}")]
    public async Task<ActionResult<UserProfileDto>> GetProfile(string phoneNumber)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.ContactInfo.PhoneNumber == phoneNumber);
        if (user == null) return NotFound();

        return new UserProfileDto
        {
            Id = user.Id,
            Name = user.Name,
            Cpf = user.Cpf,
            PhoneNumber = user.ContactInfo.PhoneNumber,
            Email = user.ContactInfo.Email,
            Address = $"{user.Address.Street}, {user.Address.City} - {user.Address.State}"
        };
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateProfile([FromBody] UserProfileDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.ContactInfo.PhoneNumber == dto.PhoneNumber);
        if (user == null)
        {
            // Create new user for demo purposes if not exists
            user = new User
            {
                Id = Guid.NewGuid(),
                ContactInfo = new ContactInfo { PhoneNumber = dto.PhoneNumber }
            };
            _context.Users.Add(user);
        }

        user.Name = dto.Name;
        user.ContactInfo.Email = dto.Email;
        // Simple address parsing for demo
        var addressParts = dto.Address.Split(',');
        if (addressParts.Length > 0) user.Address.Street = addressParts[0].Trim();
        if (addressParts.Length > 1) user.Address.City = addressParts[1].Trim();

        await _context.SaveChangesAsync();
        return Ok();
    }
}
