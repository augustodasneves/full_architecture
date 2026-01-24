using AIChatService.Models;
using Shared.Interfaces;
using AIChatService.Services;
using Shared.Events;
using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;

namespace AIChatService.Flow;

public class ConfirmingDataStateHandler : FlowStateHandlerBase
{
    private readonly ServiceBusSender _serviceBusSender;

    public ConfirmingDataStateHandler(
        IWhatsAppService whatsAppService,
        ConversationService conversationService,
        ServiceBusClient serviceBusClient,
        ILogger<ConfirmingDataStateHandler> logger) 
        : base(whatsAppService, conversationService, logger)
    {
        _serviceBusSender = serviceBusClient.CreateSender("user-update-requests");
    }

    public override string StateName => "ConfirmingData";

    public override async Task HandleAsync(ConversationState state, string text)
    {
        if (text.ToLower().Contains("sim") || text.ToLower().Contains("yes"))
        {
            var updateEvent = new UserUpdateRequestedEvent
            {
                UserId = Guid.Empty,
                NewName = state.CollectedData["NewName"],
                NewPhoneNumber = state.CollectedData["NewPhoneNumber"],
                NewEmail = state.CollectedData["NewEmail"],
                NewAddress = state.CollectedData["NewAddress"],
                WhatsAppId = state.PhoneNumber // state.PhoneNumber is actually the JID
            };

            var message = new ServiceBusMessage(JsonConvert.SerializeObject(updateEvent));
            await _serviceBusSender.SendMessageAsync(message);

            state.CurrentStep = "Idle";
            state.CollectedData.Clear();
            
            var successMessage = state.Type == FlowType.Registration 
                ? "✅ Cadastro realizado com sucesso! Bem-vindo ao nosso sistema."
                : "✅ Solicitação de atualização enviada com sucesso! Seu cadastro será atualizado em breve.";
                
            await SendAndLogMessageAsync(state, successMessage);
        }
        else
        {
            state.CurrentStep = "CollectingName";
            await SendAndLogMessageAsync(state, "Vamos começar novamente. Por favor, digite seu nome completo.");
        }
    }
}
