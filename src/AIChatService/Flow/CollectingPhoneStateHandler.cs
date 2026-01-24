using AIChatService.Models;
using Shared.Interfaces;
using AIChatService.Services;
using AIChatService.Validators;

namespace AIChatService.Flow;

public class CollectingPhoneStateHandler : FlowStateHandlerBase
{
    private readonly PhoneValidator _validator;

    public CollectingPhoneStateHandler(
        IWhatsAppService whatsAppService,
        ConversationService conversationService,
        PhoneValidator validator,
        ILogger<CollectingPhoneStateHandler> logger) 
        : base(whatsAppService, conversationService, logger)
    {
        _validator = validator;
    }

    public override string StateName => "CollectingPhone";

    public override async Task HandleAsync(ConversationState state, string text)
    {
        var validation = _validator.Validate(text);
        
        if (!validation.IsValid)
        {
            if (!state.ValidationRetries.ContainsKey("Phone"))
                state.ValidationRetries["Phone"] = 0;
            
            state.ValidationRetries["Phone"]++;
            
            if (state.ValidationRetries["Phone"] >= ConversationState.MaxRetries)
            {
                state.CurrentStep = "Idle";
                state.CollectedData.Clear();
                state.ValidationRetries.Clear();
                
                var action = state.Type == FlowType.Registration ? "cadastro" : "atualização";
                await SendAndLogMessageAsync(state, $"❌ Muitas tentativas inválidas. O processo de {action} foi cancelado. Você pode começar novamente quando quiser.");
                return;
            }
            
            var retriesLeft = ConversationState.MaxRetries - state.ValidationRetries["Phone"];
            await SendAndLogMessageAsync(state, $"{validation.ErrorMessage}\n\nTentativas restantes: {retriesLeft}");
            return;
        }
        
        state.CollectedData["NewPhoneNumber"] = validation.NormalizedValue;
        state.ValidationRetries["Phone"] = 0;
        state.CurrentStep = "CollectingEmail";
        
        var fieldName = state.Type == FlowType.Registration ? "Telefone" : "Novo telefone";
        var nextField = state.Type == FlowType.Registration ? "e-mail" : "novo e-mail";
        
        await SendAndLogMessageAsync(state, $"✅ {fieldName} salvo com sucesso!\n\nPor favor, digite seu {nextField}.");
    }
}
