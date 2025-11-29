namespace Shared.DTOs;

public class SendWhatsAppMessageRequest
{
    public string To { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class WhatsAppMessageResponse
{
    public bool Success { get; set; }
    public string? MessageId { get; set; }
    public string? Error { get; set; }
}
