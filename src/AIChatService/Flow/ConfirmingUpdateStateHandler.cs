using AIChatService.Models;
using Shared.Interfaces;
using AIChatService.Services;

namespace AIChatService.Flow;

public class ConfirmingUpdateStateHandler : FlowStateHandlerBase
{
    public ConfirmingUpdateStateHandler(
        IWhatsAppService whatsAppService,
        ConversationService conversationService,
        ILogger<ConfirmingUpdateStateHandler> logger) 
        : base(whatsAppService, conversationService, logger)
    {
    }

    public override string StateName => "ConfirmingUpdate";

    public override async Task HandleAsync(ConversationState state, string text)
    {
        if (text.ToLower().Contains("sim") || text.ToLower().Contains("yes"))
        {
            state.CurrentStep = "CollectingPhone";
            await SendAndLogMessageAsync(state, "Por favor, digite seu novo número de telefone.");
        }
        else
        {
            state.CurrentStep = "Idle";
            await SendAndLogMessageAsync(state, "Ok, cancelando a atualização.");
        }
    }
}
