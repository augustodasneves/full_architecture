using AIChatService.Models;
using Shared.Interfaces;
using AIChatService.Services;
using AIChatService.Validators;

namespace AIChatService.Flow;

public class CollectingEmailStateHandler : FlowStateHandlerBase
{
    private readonly EmailValidator _validator;

    public CollectingEmailStateHandler(
        IWhatsAppService whatsAppService,
        ConversationService conversationService,
        EmailValidator validator,
        ILogger<CollectingEmailStateHandler> logger) 
        : base(whatsAppService, conversationService, logger)
    {
        _validator = validator;
    }

    public override string StateName => "CollectingEmail";

    public override async Task HandleAsync(ConversationState state, string text)
    {
        var validation = _validator.Validate(text);
        
        if (!validation.IsValid)
        {
            if (!state.ValidationRetries.ContainsKey("Email"))
                state.ValidationRetries["Email"] = 0;
            
            state.ValidationRetries["Email"]++;
            
            if (state.ValidationRetries["Email"] >= ConversationState.MaxRetries)
            {
                state.CurrentStep = "Idle";
                state.CollectedData.Clear();
                state.ValidationRetries.Clear();
                
                var action = state.Type == FlowType.Registration ? "cadastro" : "atualização";
                await SendAndLogMessageAsync(state, $"❌ Muitas tentativas inválidas. O processo de {action} foi cancelado. Você pode começar novamente quando quiser.");
                return;
            }
            
            var retriesLeft = ConversationState.MaxRetries - state.ValidationRetries["Email"];
            await SendAndLogMessageAsync(state, $"{validation.ErrorMessage}\n\nTentativas restantes: {retriesLeft}");
            return;
        }
        
        state.CollectedData["NewEmail"] = validation.NormalizedValue;
        state.ValidationRetries["Email"] = 0;
        state.CurrentStep = "CollectingAddress";
        
        var fieldName = state.Type == FlowType.Registration ? "E-mail" : "Novo e-mail";
        var nextField = state.Type == FlowType.Registration ? "endereço completo" : "novo endereço completo";
        
        await SendAndLogMessageAsync(state, $"✅ {fieldName} salvo com sucesso!\n\nPor favor, digite seu {nextField}.");
    }
}
