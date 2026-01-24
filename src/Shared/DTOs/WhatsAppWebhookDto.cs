using System.Text.Json.Serialization;

namespace Shared.DTOs;

public class WhatsAppWebhookDto
{
    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    [JsonPropertyName("entry")]
    public List<WhatsAppEntryDto> Entry { get; set; } = new();
}

public class WhatsAppEntryDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("changes")]
    public List<WhatsAppChangeDto> Changes { get; set; } = new();
}

public class WhatsAppChangeDto
{
    [JsonPropertyName("value")]
    public WhatsAppValueDto Value { get; set; } = new();

    [JsonPropertyName("field")]
    public string Field { get; set; } = string.Empty;
}

public class WhatsAppValueDto
{
    [JsonPropertyName("messaging_product")]
    public string MessagingProduct { get; set; } = string.Empty;

    [JsonPropertyName("metadata")]
    public WhatsAppMetadataDto Metadata { get; set; } = new();

    [JsonPropertyName("contacts")]
    public List<WhatsAppContactDto> Contacts { get; set; } = new();

    [JsonPropertyName("messages")]
    public List<WhatsAppIncomingMessageDto> Messages { get; set; } = new();
}

public class WhatsAppMetadataDto
{
    [JsonPropertyName("display_phone_number")]
    public string DisplayPhoneNumber { get; set; } = string.Empty;

    [JsonPropertyName("phone_number_id")]
    public string PhoneNumberId { get; set; } = string.Empty;
}

public class WhatsAppContactDto
{
    [JsonPropertyName("profile")]
    public WhatsAppProfileInfo Profile { get; set; } = new();

    [JsonPropertyName("wa_id")]
    public string WaId { get; set; } = string.Empty;
}

public class WhatsAppProfileInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class WhatsAppIncomingMessageDto
{
    [JsonPropertyName("from")]
    public string From { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public WhatsAppTextDto? Text { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}

public class WhatsAppTextDto
{
    [JsonPropertyName("body")]
    public string Body { get; set; } = string.Empty;
}
