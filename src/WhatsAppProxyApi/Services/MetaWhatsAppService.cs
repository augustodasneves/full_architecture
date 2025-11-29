using Microsoft.Extensions.Options;
using Shared.DTOs;
using System.Text;
using System.Text.Json;
using WhatsAppProxyApi.Models;

namespace WhatsAppProxyApi.Services;

public class MetaWhatsAppService
{
    private readonly HttpClient _httpClient;
    private readonly WhatsAppSettings _settings;
    private readonly ILogger<MetaWhatsAppService> _logger;

    public MetaWhatsAppService(HttpClient httpClient, IOptions<WhatsAppSettings> settings, ILogger<MetaWhatsAppService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<WhatsAppMessageResponse> SendTextMessageAsync(string to, string message)
    {
        try
        {
            var url = $"{_settings.BaseUrl}/{_settings.ApiVersion}/{_settings.PhoneNumberId}/messages";

            var payload = new
            {
                messaging_product = "whatsapp",
                recipient_type = "individual",
                to = to,
                type = "text",
                text = new
                {
                    preview_url = false,
                    body = message
                }
            };

            var jsonContent = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.AccessToken}");

            _logger.LogInformation("Sending WhatsApp message to {To}: {Message}", to, message);

            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var messageId = result.GetProperty("messages")[0].GetProperty("id").GetString();

                _logger.LogInformation("Message sent successfully. MessageId: {MessageId}", messageId);

                return new WhatsAppMessageResponse
                {
                    Success = true,
                    MessageId = messageId
                };
            }
            else
            {
                _logger.LogError("Failed to send message. Status: {Status}, Response: {Response}", 
                    response.StatusCode, responseContent);

                return new WhatsAppMessageResponse
                {
                    Success = false,
                    Error = $"Meta API error: {response.StatusCode} - {responseContent}"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while sending WhatsApp message to {To}", to);
            return new WhatsAppMessageResponse
            {
                Success = false,
                Error = ex.Message
            };
        }
    }
}
