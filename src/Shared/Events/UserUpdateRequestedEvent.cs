namespace Shared.Events;

public class UserUpdateRequestedEvent
{
    public Guid UserId { get; set; }
    public string NewName { get; set; } = string.Empty;
    public string NewPhoneNumber { get; set; } = string.Empty;
    public string NewEmail { get; set; } = string.Empty;
    public string NewAddress { get; set; } = string.Empty;
    public string WhatsAppId { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
}
