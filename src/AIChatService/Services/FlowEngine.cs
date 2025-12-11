using AIChatService.Models;
using AIChatService.Validators;
using Shared.Events;
using Shared.Interfaces;
using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;

namespace AIChatService.Services;

public class FlowEngine
{
    private readonly ConversationService _conversationService;
    private readonly ILLMService _llmService;
    private readonly IWhatsAppService _whatsAppService;
    private readonly ServiceBusSender _serviceBusSender;
    private readonly ILogger<FlowEngine> _logger;
    private readonly PhoneValidator _phoneValidator;
    private readonly EmailValidator _emailValidator;
    private readonly AddressValidator _addressValidator;

    public FlowEngine(
        ConversationService conversationService, 
        ILLMService llmService, 
        IWhatsAppService whatsAppService,
        ServiceBusClient serviceBusClient, 
        ILogger<FlowEngine> logger,
        PhoneValidator phoneValidator,
        EmailValidator emailValidator,
        AddressValidator addressValidator)
    {
        _conversationService = conversationService;
        _llmService = llmService;
        _whatsAppService = whatsAppService;
        _serviceBusSender = serviceBusClient.CreateSender("user-update-requests");
        _logger = logger;
        _phoneValidator = phoneValidator;
        _emailValidator = emailValidator;
        _addressValidator = addressValidator;
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
            var message = "Você deseja atualizar seus dados cadastrais?";
            await _whatsAppService.SendMessageAsync(state.PhoneNumber, message);
            _logger.LogInformation("Sent to {Phone}: {Message}", state.PhoneNumber, message);
        }
        else
        {
            var message = "Posso ajudá-lo a atualizar seus dados. É só pedir!";
            await _whatsAppService.SendMessageAsync(state.PhoneNumber, message);
            _logger.LogInformation("Sent to {Phone}: {Message}", state.PhoneNumber, message);
        }
    }

    private async Task HandleConfirmingUpdate(ConversationState state, string text)
    {
        if (text.ToLower().Contains("sim") || text.ToLower().Contains("yes"))
        {
            state.CurrentStep = "CollectingPhone";
            var message = "Por favor, digite seu novo número de telefone.";
            await _whatsAppService.SendMessageAsync(state.PhoneNumber, message);
            _logger.LogInformation("Sent to {Phone}: {Message}", state.PhoneNumber, message);
        }
        else
        {
            state.CurrentStep = "Idle";
            var message = "Ok, cancelando a atualização.";
            await _whatsAppService.SendMessageAsync(state.PhoneNumber, message);
            _logger.LogInformation("Sent to {Phone}: {Message}", state.PhoneNumber, message);
        }
    }

    private async Task HandleCollectingPhone(ConversationState state, string text)
    {
        var validation = _phoneValidator.Validate(text);
        
        if (!validation.IsValid)
        {
            // Incrementa contador de tentativas
            if (!state.ValidationRetries.ContainsKey("Phone"))
                state.ValidationRetries["Phone"] = 0;
            
            state.ValidationRetries["Phone"]++;
            
            if (state.ValidationRetries["Phone"] >= ConversationState.MaxRetries)
            {
                // Excedeu tentativas, cancela o fluxo
                state.CurrentStep = "Idle";
                state.CollectedData.Clear();
                state.ValidationRetries.Clear();
                
                var message = "❌ Muitas tentativas inválidas. O processo de atualização foi cancelado. " +
                             "Você pode começar novamente quando quiser.";
                await _whatsAppService.SendMessageAsync(state.PhoneNumber, message);
                _logger.LogInformation("Sent to {Phone}: {Message}", state.PhoneNumber, message);
                _logger.LogWarning("User {Phone} exceeded max retries for phone validation", state.PhoneNumber);
                return;
            }
            
            // Envia mensagem de erro com número de tentativas restantes
            var retriesLeft = ConversationState.MaxRetries - state.ValidationRetries["Phone"];
            var errorMessage = $"{validation.ErrorMessage}\n\n" +
                             $"Tentativas restantes: {retriesLeft}";
            await _whatsAppService.SendMessageAsync(state.PhoneNumber, errorMessage);
            _logger.LogInformation("Sent to {Phone}: {Message}", state.PhoneNumber, errorMessage);
            _logger.LogInformation("Phone validation failed for {Phone}, retries: {Retries}", 
                state.PhoneNumber, state.ValidationRetries["Phone"]);
            return;
        }
        
        // Validação passou, salva valor normalizado
        state.CollectedData["NewPhoneNumber"] = validation.NormalizedValue;
        state.ValidationRetries["Phone"] = 0; // Reset contador
        state.CurrentStep = "CollectingEmail";
        
        var successMessage = "✅ Telefone salvo com sucesso!\n\nPor favor, digite seu novo e-mail.";
        await _whatsAppService.SendMessageAsync(state.PhoneNumber, successMessage);
        _logger.LogInformation("Sent to {Phone}: {Message}", state.PhoneNumber, successMessage);
    }

    private async Task HandleCollectingEmail(ConversationState state, string text)
    {
        var validation = _emailValidator.Validate(text);
        
        if (!validation.IsValid)
        {
            // Incrementa contador de tentativas
            if (!state.ValidationRetries.ContainsKey("Email"))
                state.ValidationRetries["Email"] = 0;
            
            state.ValidationRetries["Email"]++;
            
            if (state.ValidationRetries["Email"] >= ConversationState.MaxRetries)
            {
                // Excedeu tentativas, cancela o fluxo
                state.CurrentStep = "Idle";
                state.CollectedData.Clear();
                state.ValidationRetries.Clear();
                
                var message = "❌ Muitas tentativas inválidas. O processo de atualização foi cancelado. " +
                             "Você pode começar novamente quando quiser.";
                await _whatsAppService.SendMessageAsync(state.PhoneNumber, message);
                _logger.LogInformation("Sent to {Phone}: {Message}", state.PhoneNumber, message);
                _logger.LogWarning("User {Phone} exceeded max retries for email validation", state.PhoneNumber);
                return;
            }
            
            // Envia mensagem de erro com número de tentativas restantes
            var retriesLeft = ConversationState.MaxRetries - state.ValidationRetries["Email"];
            var errorMessage = $"{validation.ErrorMessage}\n\n" +
                             $"Tentativas restantes: {retriesLeft}";
            await _whatsAppService.SendMessageAsync(state.PhoneNumber, errorMessage);
            _logger.LogInformation("Sent to {Phone}: {Message}", state.PhoneNumber, errorMessage);
            _logger.LogInformation("Email validation failed for {Phone}, retries: {Retries}", 
                state.PhoneNumber, state.ValidationRetries["Email"]);
            return;
        }
        
        // Validação passou, salva valor normalizado
        state.CollectedData["NewEmail"] = validation.NormalizedValue;
        state.ValidationRetries["Email"] = 0; // Reset contador
        state.CurrentStep = "CollectingAddress";
        
        var successMessage = "✅ E-mail salvo com sucesso!\n\nPor favor, digite seu novo endereço completo.";
        await _whatsAppService.SendMessageAsync(state.PhoneNumber, successMessage);
        _logger.LogInformation("Sent to {Phone}: {Message}", state.PhoneNumber, successMessage);
    }

    private async Task HandleCollectingAddress(ConversationState state, string text)
    {
        var validation = _addressValidator.Validate(text);
        
        if (!validation.IsValid)
        {
            // Incrementa contador de tentativas
            if (!state.ValidationRetries.ContainsKey("Address"))
                state.ValidationRetries["Address"] = 0;
            
            state.ValidationRetries["Address"]++;
            
            if (state.ValidationRetries["Address"] >= ConversationState.MaxRetries)
            {
                // Excedeu tentativas, cancela o fluxo
                state.CurrentStep = "Idle";
                state.CollectedData.Clear();
                state.ValidationRetries.Clear();
                
                var message = "❌ Muitas tentativas inválidas. O processo de atualização foi cancelado. " +
                             "Você pode começar novamente quando quiser.";
                await _whatsAppService.SendMessageAsync(state.PhoneNumber, message);
                _logger.LogInformation("Sent to {Phone}: {Message}", state.PhoneNumber, message);
                _logger.LogWarning("User {Phone} exceeded max retries for address validation", state.PhoneNumber);
                return;
            }
            
            // Envia mensagem de erro com número de tentativas restantes
            var retriesLeft = ConversationState.MaxRetries - state.ValidationRetries["Address"];
            var errorMessage = $"{validation.ErrorMessage}\n\n" +
                             $"Tentativas restantes: {retriesLeft}";
            await _whatsAppService.SendMessageAsync(state.PhoneNumber, errorMessage);
            _logger.LogInformation("Address validation failed for {Phone}, retries: {Retries}", 
                state.PhoneNumber, state.ValidationRetries["Address"]);
            return;
        }
        
        // Validação passou, salva valor normalizado
        state.CollectedData["NewAddress"] = validation.NormalizedValue;
        state.ValidationRetries["Address"] = 0; // Reset contador
        state.CurrentStep = "ConfirmingData";
        
        var confirmationMessage = $"✅ Endereço salvo com sucesso!\n\n" +
                     $"Por favor, confirme seus dados:\n\n" +
                     $"📱 Telefone: {state.CollectedData["NewPhoneNumber"]}\n" +
                     $"📧 Email: {state.CollectedData["NewEmail"]}\n" +
                     $"🏠 Endereço: {state.CollectedData["NewAddress"]}\n\n" +
                     $"Está correto? (sim/não)";
        await _whatsAppService.SendMessageAsync(state.PhoneNumber, confirmationMessage);
        _logger.LogInformation("Sent confirmation request to {Phone}", state.PhoneNumber);
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
            
            var responseMessage = "✅ Solicitação de atualização enviada com sucesso! Seus dados serão atualizados em breve.";
            await _whatsAppService.SendMessageAsync(state.PhoneNumber, responseMessage);
            _logger.LogInformation("Sent to {Phone}: {Message}", state.PhoneNumber, responseMessage);
            _logger.LogInformation("Update request submitted for {Phone}", state.PhoneNumber);
        }
        else
        {
            state.CurrentStep = "CollectingPhone"; // Restart loop
            var responseMessage = "Vamos começar novamente. Por favor, digite seu novo número de telefone.";
            await _whatsAppService.SendMessageAsync(state.PhoneNumber, responseMessage);
            _logger.LogInformation("Sent to {Phone}: {Message}", state.PhoneNumber, responseMessage);
            _logger.LogInformation("Restarting data collection for {Phone}", state.PhoneNumber);
        }
    }
}
