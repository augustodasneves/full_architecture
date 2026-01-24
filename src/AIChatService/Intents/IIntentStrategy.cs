using AIChatService.Models;
using Shared.DTOs;

namespace AIChatService.Intents;

public interface IIntentStrategy
{
    string IntentName { get; }
    Task ExecuteAsync(ConversationState state, UserProfileDto? userProfile, string text);
}
