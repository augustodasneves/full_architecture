using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using Microsoft.Extensions.Options;
using WhatsAppProxyApi.Models;
using WhatsAppProxyApi.Services;
using Microsoft.AspNetCore.Authorization;

namespace WhatsAppProxyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class WhatsAppController : ControllerBase
{
    private readonly IWhatsAppService _whatsAppService;
    private readonly WhatsAppSettings _settings;
    private readonly ILogger<WhatsAppController> _logger;

    public WhatsAppController(IWhatsAppService whatsAppService, IOptions<WhatsAppSettings> settings, ILogger<WhatsAppController> logger)
    {
        _whatsAppService = whatsAppService;
        _settings = settings.Value;
        _logger = logger;
    }

    [HttpGet("webhook")]
    public IActionResult VerifyWebhook(
        [FromQuery(Name = "hub.mode")] string mode,
        [FromQuery(Name = "hub.verify_token")] string verifyToken,
        [FromQuery(Name = "hub.challenge")] string challenge)
    {
        if (mode == "subscribe" && verifyToken == _settings.VerifyToken)
        {
            _logger.LogInformation("Webhook verified successfully.");
            return Ok(challenge);
        }

        _logger.LogWarning("Webhook verification failed. Mode: {Mode}, Token: {Token}", mode, verifyToken);
        return StatusCode(403);
    }

    [HttpPost("webhook")]
    public IActionResult ReceiveWebhook([FromBody] object payload)
    {
        _logger.LogInformation("Received webhook payload: {Payload}", payload);
        return Ok();
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

    [HttpGet("qrcode")]
    public async Task<IActionResult> GetQrCode()
    {
        var (success, qrCode, message) = await _whatsAppService.GetQrCodeAsync();
        
        if (success)
        {
            return Ok(new { success = true, qr = qrCode, message });
        }
        
        return Ok(new { success = false, message });
    }

    [HttpGet("connection-status")]
    public async Task<IActionResult> GetConnectionStatus()
    {
        var (connected, state) = await _whatsAppService.GetConnectionStatusAsync();
        return Ok(new { connected, state });
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "WhatsAppProxyApi" });
    }
}
