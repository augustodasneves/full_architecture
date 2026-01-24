using AIChatService.Models;
using Shared.Interfaces;
using AIChatService.Flow;

namespace AIChatService.Services;

public class FlowEngine
{
    private readonly ConversationService _conversationService;
    private readonly IEnumerable<IFlowStateHandler> _stateHandlers;
    private readonly ILogger<FlowEngine> _logger;

    public FlowEngine(
        ConversationService conversationService, 
        IEnumerable<IFlowStateHandler> stateHandlers,
        ILogger<FlowEngine> logger)
    {
        _conversationService = conversationService;
        _stateHandlers = stateHandlers;
        _logger = logger;
    }

    public async Task ProcessMessageAsync(string jid, string text)
    {
        var state = await _conversationService.GetStateAsync(jid);
        
        // Log incoming message
        await _conversationService.LogMessageAsync(state.FlowId, new FlowMessage
        {
            MessageId = Guid.NewGuid().ToString(),
            Timestamp = DateTime.UtcNow,
            Direction = MessageDirection.Incoming,
            Content = text,
            Step = state.CurrentStep
        });

        // Resolve handler
        var handler = _stateHandlers.FirstOrDefault(h => h.StateName.Equals(state.CurrentStep, StringComparison.OrdinalIgnoreCase));
        
        if (handler != null)
        {
            _logger.LogInformation("Handling message for {Jid} in state {State}", jid, state.CurrentStep);
            await handler.HandleAsync(state, text);
        }
        else
        {
            _logger.LogWarning("No handler found for state {State}. Resetting to Idle.", state.CurrentStep);
            state.CurrentStep = "Idle";
            var idleHandler = _stateHandlers.First(h => h.StateName == "Idle");
            await idleHandler.HandleAsync(state, text);
        }

        await _conversationService.SaveStateAsync(state);
    }
}
