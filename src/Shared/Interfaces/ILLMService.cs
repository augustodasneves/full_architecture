namespace Shared.Interfaces;

public interface ILLMService
{
    Task<string> IdentifyIntentAsync(string userMessage);
    Task<string> ExtractEntityAsync(string userMessage, string entityType);
}
