namespace AIChatService.Models;

public class ConversationState
{
    public const int MaxRetries = 3;
    
    public string PhoneNumber { get; set; } = string.Empty;
    public string CurrentStep { get; set; } = "Idle";
    public Dictionary<string, string> CollectedData { get; set; } = new();
    public Dictionary<string, int> ValidationRetries { get; set; } = new();
    public int RetryCount { get; set; } = 0;
}
