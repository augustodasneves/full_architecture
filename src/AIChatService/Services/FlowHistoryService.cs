using MongoDB.Driver;
using AIChatService.Models;

namespace AIChatService.Services;

public class FlowHistoryService
{
    private readonly IMongoCollection<ConversationFlowDocument> _flows;
    private readonly DataAnonymizationService _anonymizationService;
    private readonly ILogger<FlowHistoryService> _logger;
    
    public FlowHistoryService(
        IMongoClient mongoClient,
        DataAnonymizationService anonymizationService,
        ILogger<FlowHistoryService> logger,
        IConfiguration configuration)
    {
        var database = mongoClient.GetDatabase(configuration["MongoDB:DatabaseName"] ?? "whatsapp_flows");
        _flows = database.GetCollection<ConversationFlowDocument>("conversation_flows");
        _anonymizationService = anonymizationService;
        _logger = logger;
        
        CreateIndexes();
    }
    
    private void CreateIndexes()
    {
        try
        {
            // Índice por FlowId (busca rápida)
            var flowIdIndex = Builders<ConversationFlowDocument>.IndexKeys.Ascending(x => x.FlowId);
            _flows.Indexes.CreateOne(new CreateIndexModel<ConversationFlowDocument>(flowIdIndex));
            
            // Índice por PhoneNumberHash (buscar histórico de um contato)
            var phoneHashIndex = Builders<ConversationFlowDocument>.IndexKeys.Ascending(x => x.PhoneNumberHash);
            _flows.Indexes.CreateOne(new CreateIndexModel<ConversationFlowDocument>(phoneHashIndex));
            
            // Índice TTL para expiração automática (90 dias)
            var expireIndex = Builders<ConversationFlowDocument>.IndexKeys.Ascending(x => x.ExpiresAt);
            var expireOptions = new CreateIndexOptions { ExpireAfter = TimeSpan.Zero };
            _flows.Indexes.CreateOne(new CreateIndexModel<ConversationFlowDocument>(expireIndex, expireOptions));
            
            _logger.LogInformation("MongoDB indexes created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not create MongoDB indexes (may already exist)");
        }
    }
    
    // Cria um novo fluxo de atendimento no MongoDB
    public async Task<string> CreateNewFlowAsync(string phoneNumber)
    {
        var flowId = _anonymizationService.GenerateFlowId(phoneNumber);
        var phoneHash = _anonymizationService.HashData(phoneNumber);
        
        var newFlow = new ConversationFlowDocument
        {
            FlowId = flowId,
            PhoneNumberHash = phoneHash,
            MaskedPhoneNumber = _anonymizationService.MaskPhoneNumber(phoneNumber),
            CurrentStep = "Idle",
            CollectedData = new Dictionary<string, string>(),
            ValidationRetries = new Dictionary<string, int>(),
            Messages = new List<FlowMessage>(),
            CreatedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(90), // Expira em 90 dias
            Status = FlowStatus.Active
        };
        
        await _flows.InsertOneAsync(newFlow);
        
        _logger.LogInformation("Created new flow {FlowId} for contact {MaskedPhone}", 
            flowId, newFlow.MaskedPhoneNumber);
        
        return flowId;
    }
    
    // Adiciona mensagem ao fluxo
    public async Task AddMessageToFlowAsync(string flowId, FlowMessage message)
    {
        var filter = Builders<ConversationFlowDocument>.Filter.Eq(x => x.FlowId, flowId);
        var update = Builders<ConversationFlowDocument>.Update
            .Push(x => x.Messages, message)
            .Set(x => x.LastUpdatedAt, DateTime.UtcNow);
        
        await _flows.UpdateOneAsync(filter, update);
    }
    
    // Atualiza estado do fluxo no MongoDB
    public async Task UpdateFlowStateAsync(string flowId, ConversationState state)
    {
        // Anonimizar dados coletados
        var anonymizedData = new Dictionary<string, string>();
        foreach (var kvp in state.CollectedData)
        {
            anonymizedData[kvp.Key] = kvp.Key switch
            {
                "NewPhoneNumber" => _anonymizationService.MaskPhoneNumber(kvp.Value),
                "NewEmail" => _anonymizationService.MaskEmail(kvp.Value),
                _ => kvp.Value
            };
        }
        
        var filter = Builders<ConversationFlowDocument>.Filter.Eq(x => x.FlowId, flowId);
        var update = Builders<ConversationFlowDocument>.Update
            .Set(x => x.CurrentStep, state.CurrentStep)
            .Set(x => x.CollectedData, anonymizedData)
            .Set(x => x.ValidationRetries, state.ValidationRetries)
            .Set(x => x.LastUpdatedAt, DateTime.UtcNow);
        
        await _flows.UpdateOneAsync(filter, update);
    }
    
    // Finaliza um fluxo
    public async Task CompleteFlowAsync(string flowId, FlowStatus status)
    {
        var filter = Builders<ConversationFlowDocument>.Filter.Eq(x => x.FlowId, flowId);
        var update = Builders<ConversationFlowDocument>.Update
            .Set(x => x.Status, status)
            .Set(x => x.LastUpdatedAt, DateTime.UtcNow);
        
        await _flows.UpdateOneAsync(filter, update);
        
        _logger.LogInformation("Flow {FlowId} completed with status {Status}", flowId, status);
    }
    
    // Busca histórico de fluxos de um contato
    public async Task<List<ConversationFlowDocument>> GetContactFlowHistoryAsync(string phoneNumber, int limit = 10)
    {
        var phoneHash = _anonymizationService.HashData(phoneNumber);
        var filter = Builders<ConversationFlowDocument>.Filter.Eq(x => x.PhoneNumberHash, phoneHash);
        var sort = Builders<ConversationFlowDocument>.Sort.Descending(x => x.CreatedAt);
        
        return await _flows.Find(filter).Sort(sort).Limit(limit).ToListAsync();
    }
}
