using AIChatService.Models;
using Shared.Events;
using Shared.Interfaces;
using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;

namespace AIChatService.Services;

public class FlowEngine
{
    private readonly ConversationService _conversationService;
    private readonly ILLMService _llmService;
    private readonly ServiceBusSender _serviceBusSender;
    private readonly ILogger<FlowEngine> _logger;

    public FlowEngine(ConversationService conversationService, ILLMService llmService, ServiceBusClient serviceBusClient, ILogger<FlowEngine> logger)
    {
        _conversationService = conversationService;
        _llmService = llmService;
        _serviceBusSender = serviceBusClient.CreateSender("user-update-requests");
        _logger = logger;
    }

    public async Task ProcessMessageAsync(string from, string text)
    {
        var state = await _conversationService.GetStateAsync(from);

        switch (state.CurrentStep)
        {
            case "Idle":
                await HandleIdleState(state, text);
                break;
            case "ConfirmingUpdate":
                await HandleConfirmingUpdate(state, text);
                break;
            case "CollectingPhone":
                await HandleCollectingPhone(state, text);
                break;
            case "CollectingEmail":
                await HandleCollectingEmail(state, text);
                break;
            case "CollectingAddress":
                await HandleCollectingAddress(state, text);
                break;
            case "ConfirmingData":
                await HandleConfirmingData(state, text);
                break;
        }

        await _conversationService.SaveStateAsync(state);
    }

    private async Task HandleIdleState(ConversationState state, string text)
    {
        var intent = await _llmService.IdentifyIntentAsync(text);
        if (intent.Contains("UPDATE_REGISTRATION"))
        {
            state.CurrentStep = "ConfirmingUpdate";
            // Send message: "Do you want to update your registration data?"
            _logger.LogInformation("Sending to {Phone}: Do you want to update your registration data?", state.PhoneNumber);
        }
        else
        {
            // Send default message
             _logger.LogInformation("Sending to {Phone}: I can help you update your data. Just ask.", state.PhoneNumber);
        }
    }

    private async Task HandleConfirmingUpdate(ConversationState state, string text)
    {
        if (text.ToLower().Contains("sim") || text.ToLower().Contains("yes"))
        {
            state.CurrentStep = "CollectingPhone";
            _logger.LogInformation("Sending to {Phone}: Please enter your new phone number.", state.PhoneNumber);
        }
        else
        {
            state.CurrentStep = "Idle";
             _logger.LogInformation("Sending to {Phone}: Okay, cancelling.", state.PhoneNumber);
        }
    }

    private async Task HandleCollectingPhone(ConversationState state, string text)
    {
        state.CollectedData["NewPhoneNumber"] = text; // Validate here
        state.CurrentStep = "CollectingEmail";
        _logger.LogInformation("Sending to {Phone}: Please enter your new email.", state.PhoneNumber);
    }

    private async Task HandleCollectingEmail(ConversationState state, string text)
    {
        state.CollectedData["NewEmail"] = text; // Validate here
        state.CurrentStep = "CollectingAddress";
        _logger.LogInformation("Sending to {Phone}: Please enter your new address.", state.PhoneNumber);
    }

    private async Task HandleCollectingAddress(ConversationState state, string text)
    {
        state.CollectedData["NewAddress"] = text;
        state.CurrentStep = "ConfirmingData";
        
        var summary = $"Phone: {state.CollectedData["NewPhoneNumber"]}, Email: {state.CollectedData["NewEmail"]}, Address: {state.CollectedData["NewAddress"]}";
        _logger.LogInformation("Sending to {Phone}: Confirm these details? {Summary}", state.PhoneNumber, summary);
    }

    private async Task HandleConfirmingData(ConversationState state, string text)
    {
        if (text.ToLower().Contains("sim") || text.ToLower().Contains("yes"))
        {
            // Publish event
            var updateEvent = new UserUpdateRequestedEvent
            {
                UserId = Guid.Empty, // In real app, resolve from phone
                NewPhoneNumber = state.CollectedData["NewPhoneNumber"],
                NewEmail = state.CollectedData["NewEmail"],
                NewAddress = state.CollectedData["NewAddress"]
            };

            var message = new ServiceBusMessage(JsonConvert.SerializeObject(updateEvent));
            await _serviceBusSender.SendMessageAsync(message);

            state.CurrentStep = "Idle";
            state.CollectedData.Clear();
            _logger.LogInformation("Sending to {Phone}: Update request submitted successfully.", state.PhoneNumber);
        }
        else
        {
            state.CurrentStep = "CollectingPhone"; // Restart loop
             _logger.LogInformation("Sending to {Phone}: Let's start over. Phone number?", state.PhoneNumber);
        }
    }
}
