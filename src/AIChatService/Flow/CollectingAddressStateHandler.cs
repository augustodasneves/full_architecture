using AIChatService.Models;
using Shared.Interfaces;
using AIChatService.Services;
using AIChatService.Validators;

namespace AIChatService.Flow;

public class CollectingAddressStateHandler : FlowStateHandlerBase
{
    private readonly AddressValidator _validator;

    public CollectingAddressStateHandler(
        IWhatsAppService whatsAppService,
        ConversationService conversationService,
        AddressValidator validator,
        ILogger<CollectingAddressStateHandler> logger) 
        : base(whatsAppService, conversationService, logger)
    {
        _validator = validator;
    }

    public override string StateName => "CollectingAddress";

    public override async Task HandleAsync(ConversationState state, string text)
    {
        var validation = _validator.Validate(text);
        
        if (!validation.IsValid)
        {
            if (!state.ValidationRetries.ContainsKey("Address"))
                state.ValidationRetries["Address"] = 0;
            
            state.ValidationRetries["Address"]++;
            
            if (state.ValidationRetries["Address"] >= ConversationState.MaxRetries)
            {
                state.CurrentStep = "Idle";
                state.CollectedData.Clear();
                state.ValidationRetries.Clear();
                
                var action = state.Type == FlowType.Registration ? "cadastro" : "atualização";
                await SendAndLogMessageAsync(state, $"❌ Muitas tentativas inválidas. O processo de {action} foi cancelado. Você pode começar novamente quando quiser.");
                return;
            }
            
            var retriesLeft = ConversationState.MaxRetries - state.ValidationRetries["Address"];
            await SendAndLogMessageAsync(state, $"{validation.ErrorMessage}\n\nTentativas restantes: {retriesLeft}");
            return;
        }
        
        state.CollectedData["NewAddress"] = validation.NormalizedValue;
        state.ValidationRetries["Address"] = 0;
        state.CurrentStep = "ConfirmingData";
        
        var title = state.Type == FlowType.Registration ? "confirme seus dados de cadastro" : "confirme seus novos dados";
        
        await SendAndLogMessageAsync(state, $"✅ Endereço salvo com sucesso!\n\nPor favor, {title}:\n\n" +
                     $"👤 Nome: {state.CollectedData["NewName"]}\n" +
                     $"📱 Telefone: {state.CollectedData["NewPhoneNumber"]}\n" +
                     $"📧 Email: {state.CollectedData["NewEmail"]}\n" +
                     $"🏠 Endereço: {state.CollectedData["NewAddress"]}\n\n" +
                     $"Está correto? (sim/não)");
    }
}
