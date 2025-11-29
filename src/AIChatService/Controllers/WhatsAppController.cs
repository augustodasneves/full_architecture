using AIChatService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIChatService.Controllers;

[ApiController]
[Route("api/whatsapp")]
public class WhatsAppController : ControllerBase
{
    private readonly FlowEngine _flowEngine;
    private readonly ILogger<WhatsAppController> _logger;

    public WhatsAppController(FlowEngine flowEngine, ILogger<WhatsAppController> logger)
    {
        _flowEngine = flowEngine;
        _logger = logger;
    }

    [HttpGet("webhook")]
    public IActionResult VerifyWebhook([FromQuery(Name = "hub.mode")] string mode,
                                       [FromQuery(Name = "hub.verify_token")] string token,
                                       [FromQuery(Name = "hub.challenge")] string challenge)
    {
        // In a real app, validate the token against configuration
        if (mode == "subscribe" && token == "mytesttoken")
        {
            return Ok(int.Parse(challenge));
        }
        return Forbid();
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> ReceiveMessage([FromBody] dynamic payload)
    {
        _logger.LogInformation("Received webhook: {Payload}", (string)payload.ToString());

        // Simplified parsing for demo - assumes text message structure
        try
        {
            // Navigate dynamic json to find message body and from number
            // This is highly dependent on Meta's API structure
            var entry = payload.entry[0];
            var changes = entry.changes[0];
            var value = changes.value;
            
            if (value.messages != null)
            {
                var message = value.messages[0];
                string from = message.from;
                string text = message.text.body;

                await _flowEngine.ProcessMessageAsync(from, text);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook");
        }

        return Ok();
    }
}
