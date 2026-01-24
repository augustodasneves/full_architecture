using AIChatService.Models;
using Shared.Interfaces;
using AIChatService.Services;
using Shared.DTOs;

namespace AIChatService.Intents;

public class UpdateRegistrationStrategy : IIntentStrategy
{
    private readonly IWhatsAppService _whatsAppService;
    private readonly ConversationService _conversationService;
    private readonly ILogger<UpdateRegistrationStrategy> _logger;

    public UpdateRegistrationStrategy(
        IWhatsAppService whatsAppService,
        ConversationService conversationService,
        ILogger<UpdateRegistrationStrategy> logger)
    {
        _whatsAppService = whatsAppService;
        _conversationService = conversationService;
        _logger = logger;
    }

    public string IntentName => "UPDATE_REGISTRATION";

    public async Task ExecuteAsync(ConversationState state, UserProfileDto? userProfile, string text)
    {
        state.Type = FlowType.Update;
        state.CurrentStep = "CollectingName";
        
        var name = userProfile?.Name ?? "amigo";
        var message = $"Olá, {name}! 👋\n\nQue bom falar com você novamente. Para atualizar seus dados, primeiro vamos confirmar seu nome completo.";
        
        await _whatsAppService.SendMessageAsync(state.PhoneNumber, message);
        await _conversationService.LogMessageAsync(state.FlowId, new FlowMessage
        {
            MessageId = Guid.NewGuid().ToString(),
            Timestamp = DateTime.UtcNow,
            Direction = MessageDirection.Outgoing,
            Content = message,
            Step = state.CurrentStep
        });
    }
}
