using AIChatService.Models;
using Shared.Interfaces;
using AIChatService.Services;
using AIChatService.Intents;
using Shared.DTOs;
using Microsoft.Extensions.Logging;

namespace AIChatService.Flow;

public class IdleStateHandler : FlowStateHandlerBase
{
    private readonly IUserAccountService _userAccountService;
    private readonly ILLMService _llmService;
    private readonly IEnumerable<IIntentStrategy> _intentStrategies;

    public IdleStateHandler(
        IWhatsAppService whatsAppService,
        ConversationService conversationService,
        IUserAccountService userAccountService,
        ILLMService llmService,
        IEnumerable<IIntentStrategy> intentStrategies,
        ILogger<IdleStateHandler> logger) 
        : base(whatsAppService, conversationService, logger)
    {
        _userAccountService = userAccountService;
        _llmService = llmService;
        _intentStrategies = intentStrategies;
    }

    public override string StateName => "Idle";

    public override async Task HandleAsync(ConversationState state, string text)
    {
        var userProfile = await _userAccountService.GetUserProfileByWhatsAppIdAsync(state.PhoneNumber);
        var intentName = await _llmService.IdentifyIntentAsync(text);
        
        _logger.LogInformation("Intent identified for {Phone}: {Intent}", state.PhoneNumber, intentName);

        var strategy = _intentStrategies.FirstOrDefault(s => intentName.Contains(s.IntentName, StringComparison.OrdinalIgnoreCase)) 
                       ?? _intentStrategies.First(s => s.IntentName == "OTHER");

        _logger.LogInformation("Selected strategy: {Strategy}", strategy.GetType().Name);

        await strategy.ExecuteAsync(state, userProfile, text);
    }
}
