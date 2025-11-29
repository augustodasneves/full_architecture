using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using WhatsAppProxyApi.Services;

namespace WhatsAppProxyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WhatsAppController : ControllerBase
{
    private readonly MetaWhatsAppService _whatsAppService;
    private readonly ILogger<WhatsAppController> _logger;

    public WhatsAppController(MetaWhatsAppService whatsAppService, ILogger<WhatsAppController> logger)
    {
        _whatsAppService = whatsAppService;
        _logger = logger;
    }

    [HttpPost("send")]
    public async Task<ActionResult<WhatsAppMessageResponse>> SendMessage([FromBody] SendWhatsAppMessageRequest request)
    {
        if (string.IsNullOrEmpty(request.To) || string.IsNullOrEmpty(request.Message))
        {
            return BadRequest(new WhatsAppMessageResponse
            {
                Success = false,
                Error = "To and Message fields are required"
            });
        }

        _logger.LogInformation("Received request to send message to {To}", request.To);

        var result = await _whatsAppService.SendTextMessageAsync(request.To, request.Message);

        if (result.Success)
        {
            return Ok(result);
        }
        else
        {
            return StatusCode(500, result);
        }
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "WhatsAppProxyApi" });
    }
}
