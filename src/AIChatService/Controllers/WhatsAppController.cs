using AIChatService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace AIChatService.Controllers;

[ApiController]
[Route("api/whatsapp")]
[AllowAnonymous]
public class WhatsAppController : ControllerBase
{
    private readonly Azure.Messaging.ServiceBus.ServiceBusClient _serviceBusClient;
    private readonly ILogger<WhatsAppController> _logger;

    public WhatsAppController(Azure.Messaging.ServiceBus.ServiceBusClient serviceBusClient, ILogger<WhatsAppController> logger)
    {
        _serviceBusClient = serviceBusClient;
        _logger = logger;
    }

    [HttpGet("webhook")]
    public IActionResult VerifyWebhook([FromQuery(Name = "hub.mode")] string mode,
                                       [FromQuery(Name = "hub.verify_token")] string token,
                                       [FromQuery(Name = "hub.challenge")] string challenge)
    {
        if (mode == "subscribe" && token == "mytesttoken")
        {
            return Ok(challenge);
        }
        return Forbid();
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> ReceiveMessage([FromBody] Shared.DTOs.WhatsAppWebhookDto payload)
    {
        try
        {
            var message = payload.Entry?.FirstOrDefault()?.Changes?.FirstOrDefault()?.Value?.Messages?.FirstOrDefault();
            
            if (message != null && !string.IsNullOrEmpty(message.From) && message.Text != null)
            {
                string from = message.From;
                string text = message.Text.Body;

                if (from.EndsWith("@g.us") || from.EndsWith("@broadcast") || from.EndsWith("@newsletter"))
                {
                    return Ok();
                }

                _logger.LogInformation("Enqueuing message from {From} to Service Bus", from);

                var sender = _serviceBusClient.CreateSender("whatsapp-messages");
                var eventData = new Shared.Events.WhatsAppMessageReceivedEvent
                {
                    From = from,
                    Content = text
                };
                
                var json = System.Text.Json.JsonSerializer.Serialize(eventData);
                var busMessage = new Azure.Messaging.ServiceBus.ServiceBusMessage(json);
                
                await sender.SendMessageAsync(busMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enqueuing message to Service Bus");
        }

        return Ok();
    }
}
