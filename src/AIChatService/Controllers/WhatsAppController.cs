using AIChatService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace AIChatService.Controllers;

[ApiController]
[Route("api/whatsapp")]
[AllowAnonymous]
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
    public async Task<IActionResult> ReceiveMessage([FromBody] System.Text.Json.JsonElement payload)
    {
        _logger.LogInformation("Received webhook: {Payload}", payload.ToString());

        // Simplified parsing for demo - assumes text message structure
        try
        {
            // Navigate JSON to find message body and from number
            // This is highly dependent on Meta's API structure
            if (payload.TryGetProperty("entry", out var entryArray) && 
                entryArray.ValueKind == System.Text.Json.JsonValueKind.Array &&
                entryArray.GetArrayLength() > 0)
            {
                var entry = entryArray[0];
                
                if (entry.TryGetProperty("changes", out var changesArray) &&
                    changesArray.ValueKind == System.Text.Json.JsonValueKind.Array &&
                    changesArray.GetArrayLength() > 0)
                {
                    var changes = changesArray[0];
                    
                    if (changes.TryGetProperty("value", out var value) &&
                        value.TryGetProperty("messages", out var messagesArray) &&
                        messagesArray.ValueKind == System.Text.Json.JsonValueKind.Array &&
                        messagesArray.GetArrayLength() > 0)
                    {
                        var message = messagesArray[0];
                        
                        if (message.TryGetProperty("from", out var fromElement) &&
                            message.TryGetProperty("text", out var textElement) &&
                            textElement.TryGetProperty("body", out var bodyElement))
                        {
                            string from = fromElement.GetString() ?? "";
                            string text = bodyElement.GetString() ?? "";

                            await _flowEngine.ProcessMessageAsync(from, text);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook");
        }

        return Ok();
    }
}
