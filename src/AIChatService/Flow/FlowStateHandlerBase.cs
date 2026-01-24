using AIChatService.Models;
using Shared.Interfaces;
using AIChatService.Services;

namespace AIChatService.Flow;

public abstract class FlowStateHandlerBase : IFlowStateHandler
{
    private readonly IWhatsAppService _whatsAppService;
    private readonly ConversationService _conversationService;
    private readonly ILogger _logger;

    protected FlowStateHandlerBase(
        IWhatsAppService whatsAppService,
        ConversationService conversationService,
        ILogger logger)
    {
        _whatsAppService = whatsAppService;
        _conversationService = conversationService;
        _logger = logger;
    }

    public abstract string StateName { get; }
    public abstract Task HandleAsync(ConversationState state, string text);

    protected async Task SendAndLogMessageAsync(ConversationState state, string message)
    {
        await _whatsAppService.SendMessageAsync(state.PhoneNumber, message);
        await _conversationService.LogMessageAsync(state.FlowId, new FlowMessage
        {
            MessageId = Guid.NewGuid().ToString(),
            Timestamp = DateTime.UtcNow,
            Direction = MessageDirection.Outgoing,
            Content = message,
            Step = state.CurrentStep
        });
        _logger.LogInformation("Sent to {Phone}: {Message}", state.PhoneNumber, message);
    }
}
