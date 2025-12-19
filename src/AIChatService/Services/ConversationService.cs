using AIChatService.Models;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace AIChatService.Services;

public class ConversationService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly FlowHistoryService _flowHistoryService;
    private readonly ILogger<ConversationService> _logger;

    public ConversationService(
        IConnectionMultiplexer redis,
        FlowHistoryService flowHistoryService,
        ILogger<ConversationService> logger)
    {
        _redis = redis;
        _flowHistoryService = flowHistoryService;
        _logger = logger;
    }

    public async Task<ConversationState> GetStateAsync(string phoneNumber)
    {
        var db = _redis.GetDatabase();
        var json = await db.StringGetAsync($"conversation:{phoneNumber}");
        
        if (json.IsNullOrEmpty)
        {
            // Criar novo fluxo no MongoDB
            var flowId = await _flowHistoryService.CreateNewFlowAsync(phoneNumber);
            
            _logger.LogInformation("Created new flow {FlowId} for {Phone}", flowId, phoneNumber);
            
            return new ConversationState 
            { 
                FlowId = flowId,
                PhoneNumber = phoneNumber 
            };
        }
        
        return JsonConvert.DeserializeObject<ConversationState>(json!)!;
    }

    public async Task SaveStateAsync(ConversationState state)
    {
        var db = _redis.GetDatabase();
        var json = JsonConvert.SerializeObject(state);
        
        // Salva no Redis (cache)
        await db.StringSetAsync($"conversation:{state.PhoneNumber}", json, TimeSpan.FromMinutes(30));
        
        // Atualiza MongoDB (permanente)
        await _flowHistoryService.UpdateFlowStateAsync(state.FlowId, state);
        
        // Se fluxo foi completado/cancelado, finaliza no MongoDB
        if (state.CurrentStep == "Idle" && state.CollectedData.Count > 0)
        {
            await _flowHistoryService.CompleteFlowAsync(state.FlowId, FlowStatus.Completed);
            
            // Limpa Redis para próxima conversa criar novo fluxo
            await db.KeyDeleteAsync($"conversation:{state.PhoneNumber}");
            
            _logger.LogInformation("Flow {FlowId} completed and cleared from Redis", state.FlowId);
        }
    }
    
    // Adiciona mensagem ao histórico do fluxo
    public async Task LogMessageAsync(string flowId, FlowMessage message)
    {
        await _flowHistoryService.AddMessageToFlowAsync(flowId, message);
    }
}

