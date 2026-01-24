using AIChatService.Models;

namespace AIChatService.Flow;

public interface IFlowStateHandler
{
    string StateName { get; }
    Task HandleAsync(ConversationState state, string text);
}
