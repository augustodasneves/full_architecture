using AIChatService.Models;
using Shared.Interfaces;
using AIChatService.Services;
using Shared.DTOs;

namespace AIChatService.Intents;

public class OtherIntentStrategy : IIntentStrategy
{
    private readonly IWhatsAppService _whatsAppService;
    private readonly ConversationService _conversationService;

    public OtherIntentStrategy(IWhatsAppService whatsAppService, ConversationService conversationService)
    {
        _whatsAppService = whatsAppService;
        _conversationService = conversationService;
    }

    public string IntentName => "OTHER";

    public async Task ExecuteAsync(ConversationState state, UserProfileDto? userProfile, string text)
    {
        string message;
        if (userProfile != null)
        {
            if (text.Contains("iniciar", StringComparison.OrdinalIgnoreCase) || 
                text.Contains("começar", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("atualizar", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("mudar", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("alterar", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("cadastro", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("start", StringComparison.OrdinalIgnoreCase))
            {
                state.Type = FlowType.Update;
                state.CurrentStep = "CollectingName";
                message = $"Olá, {userProfile.Name}! 👋\n\nQue bom falar com você novamente. Para atualizar seus dados, primeiro vamos confirmar seu nome completo.";
            }
            else
            {
                message = $"Olá, {userProfile.Name}! 👋\n\nSou seu assistente virtual. Como posso ajudar hoje? Se precisar atualizar seu endereço, telefone ou e-mail, é só me avisar!";
            }
        }
        else
        {
            state.Type = FlowType.Registration;
            state.CurrentStep = "CollectingName";
            message = "Olá! 👋\n\nNotei que você ainda não tem cadastro conosco. Vamos realizar seu cadastro agora? É rápido!\n\nPara começar, por favor, digite seu nome completo.";
        }

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
