using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using UserAccountApi.Services;

namespace UserAccountApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserProfileDto>> RegisterUser([FromBody] CreateUserDto dto)
    {
        var result = await _userService.RegisterUserAsync(dto);
        
        if (!result.Success)
        {
            return Conflict(new { message = result.Message });
        }

        return CreatedAtAction(
            nameof(GetProfile), 
            new { phoneNumber = result.Profile!.PhoneNumber }, 
            result.Profile);
    }

    [HttpGet("me/{phoneNumber}")]
    public async Task<ActionResult<UserProfileDto>> GetProfile(string phoneNumber)
    {
        var profile = await _userService.GetProfileByPhoneNumberAsync(phoneNumber);
        
        if (profile == null)
        {
            return NotFound();
        }

        return Ok(profile);
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateProfile([FromBody] UserProfileDto dto)
    {
        await _userService.UpdateProfileAsync(dto);
        return Ok();
    }
}
