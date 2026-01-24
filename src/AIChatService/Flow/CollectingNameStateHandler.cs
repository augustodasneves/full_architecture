using AIChatService.Models;
using Shared.Interfaces;
using AIChatService.Services;

namespace AIChatService.Flow;

public class CollectingNameStateHandler : FlowStateHandlerBase
{
    public CollectingNameStateHandler(
        IWhatsAppService whatsAppService,
        ConversationService conversationService,
        ILogger<CollectingNameStateHandler> logger) 
        : base(whatsAppService, conversationService, logger)
    {
    }

    public override string StateName => "CollectingName";

    public override async Task HandleAsync(ConversationState state, string text)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length < 2)
        {
            await SendAndLogMessageAsync(state, "❌ Por favor, digite seu nome completo (mínimo 2 letras).");
            return;
        }

        state.CollectedData["NewName"] = text.Trim();
        state.CurrentStep = "CollectingPhone";
        
        var action = state.Type == FlowType.Registration ? "salvo" : "atualizado";
        await SendAndLogMessageAsync(state, $"✅ Nome {action} com sucesso!\n\nAgora, por favor, digite seu número de telefone (com DDD).");
    }
}
