using AIChatService.Models;
using Shared.Interfaces;
using AIChatService.Services;

namespace AIChatService.Flow;

public class IdleStateHandler : FlowStateHandlerBase
{
    private readonly IUserAccountService _userAccountService;
    private readonly ILLMService _llmService;

    public IdleStateHandler(
        IWhatsAppService whatsAppService,
        ConversationService conversationService,
        IUserAccountService userAccountService,
        ILLMService llmService,
        ILogger<IdleStateHandler> logger) 
        : base(whatsAppService, conversationService, logger)
    {
        _userAccountService = userAccountService;
        _llmService = llmService;
    }

    public override string StateName => "Idle";

    public override async Task HandleAsync(ConversationState state, string text)
    {
        var userProfile = await _userAccountService.GetUserProfileAsync(state.PhoneNumber);
        
        if (userProfile != null)
        {
            var intent = await _llmService.IdentifyIntentAsync(text);
            if (intent.Contains("UPDATE_REGISTRATION"))
            {
                state.Type = FlowType.Update;
                state.CurrentStep = "ConfirmingUpdate";
                await SendAndLogMessageAsync(state, $"Olá, {userProfile.Name}! 👋\n\nQue bom falar com você novamente. Gostaria de atualizar seus dados cadastrais?");
            }
            else
            {
                await SendAndLogMessageAsync(state, $"Olá, {userProfile.Name}! 👋\n\nSou seu assistente virtual. Como posso ajudar hoje? Se precisar atualizar seu endereço, telefone ou e-mail, é só me avisar!");
            }
        }
        else
        {
            state.Type = FlowType.Registration;
            state.CurrentStep = "CollectingPhone";
            await SendAndLogMessageAsync(state, "Olá! 👋\n\nNotei que você ainda não tem cadastro conosco. Vamos realizar seu cadastro agora? É rápido!\n\nPara começar, por favor, digite seu número de telefone (com DDD).");
        }
    }
}
