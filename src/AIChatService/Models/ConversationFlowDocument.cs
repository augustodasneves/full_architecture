using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AIChatService.Models;

public class ConversationFlowDocument
{
    [BsonId]
    public string Id { get; set; } = string.Empty;
    
    // ID único deste fluxo de atendimento
    public string FlowId { get; set; } = string.Empty;
    
    // Hash do número de telefone para anonimização
    public string PhoneNumberHash { get; set; } = string.Empty;
    
    // Número original (mascarado) apenas para referência visual
    public string MaskedPhoneNumber { get; set; } = string.Empty;
    
    public string CurrentStep { get; set; } = "Idle";
    
    // Dados coletados (anonimizados)
    public Dictionary<string, string> CollectedData { get; set; } = new();
    
    public Dictionary<string, int> ValidationRetries { get; set; } = new();
    
    // Array de mensagens deste fluxo
    public List<FlowMessage> Messages { get; set; } = new();
    
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    
    // TTL - automaticamente expirar documentos antigos (90 dias)
    public DateTime ExpiresAt { get; set; }
    
    // Status do fluxo
    public FlowStatus Status { get; set; }
}

public class FlowMessage
{
    public string MessageId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public MessageDirection Direction { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Step { get; set; } = string.Empty; // Em qual step estava quando enviou/recebeu
}

public enum MessageDirection
{
    Incoming,  // Do usuário para o bot
    Outgoing   // Do bot para o usuário
}

public enum FlowStatus
{
    Active,     // Fluxo em andamento
    Completed,  // Fluxo finalizado com sucesso
    Cancelled,  // Fluxo cancelado pelo usuário
    Expired     // Fluxo expirado por inatividade
}
