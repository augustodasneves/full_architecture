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

    [HttpPost("register")]
    public async Task<ActionResult<UserProfileDto>> RegisterUser([FromBody] CreateUserDto dto)
    {
        // Validate if user already exists by phone number
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.ContactInfo.PhoneNumber == dto.PhoneNumber);
        
        if (existingUser != null)
        {
            return Conflict(new { message = "Usuário com este número de telefone já existe." });
        }

        // Validate if CPF already exists
        if (!string.IsNullOrEmpty(dto.Cpf))
        {
            var existingCpf = await _context.Users
                .FirstOrDefaultAsync(u => u.Cpf == dto.Cpf);
            
            if (existingCpf != null)
            {
                return Conflict(new { message = "Usuário com este CPF já existe." });
            }
        }

        // Create new user
        var newUser = new User
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Cpf = dto.Cpf,
            ContactInfo = new ContactInfo
            {
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email
            },
            Address = new Address
            {
                Street = dto.Street,
                City = dto.City,
                State = dto.State,
                ZipCode = dto.ZipCode
            }
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        var response = new UserProfileDto
        {
            Id = newUser.Id,
            Name = newUser.Name,
            Cpf = newUser.Cpf,
            PhoneNumber = newUser.ContactInfo.PhoneNumber,
            Email = newUser.ContactInfo.Email,
            Address = $"{newUser.Address.Street}, {newUser.Address.City} - {newUser.Address.State}"
        };

        return CreatedAtAction(nameof(GetProfile), new { phoneNumber = newUser.ContactInfo.PhoneNumber }, response);
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
