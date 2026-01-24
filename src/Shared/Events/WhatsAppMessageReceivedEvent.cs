namespace Shared.Events;

public class WhatsAppMessageReceivedEvent
{
    public string From { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}
