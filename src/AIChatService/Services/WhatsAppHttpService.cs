using Shared.DTOs;
using Shared.Interfaces;
using System.Text;
using System.Text.Json;

namespace AIChatService.Services;

public class WhatsAppHttpService : IWhatsAppService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WhatsAppHttpService> _logger;

    public WhatsAppHttpService(HttpClient httpClient, ILogger<WhatsAppHttpService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<WhatsAppMessageResponse> SendMessageAsync(string to, string message)
    {
        try
        {
            var request = new SendWhatsAppMessageRequest
            {
                To = to,
                Message = message
            };

            var jsonContent = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            _logger.LogInformation("Sending message to {To} via WhatsApp Proxy", to);

            var response = await _httpClient.PostAsync("/api/whatsapp/send", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<WhatsAppMessageResponse>(responseContent);
                return result ?? new WhatsAppMessageResponse { Success = false, Error = "Invalid response" };
            }
            else
            {
                _logger.LogError("Failed to send message via proxy. Status: {Status}", response.StatusCode);
                return new WhatsAppMessageResponse
                {
                    Success = false,
                    Error = $"Proxy error: {response.StatusCode}"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while sending message via WhatsApp Proxy");
            return new WhatsAppMessageResponse
            {
                Success = false,
                Error = ex.Message
            };
        }
    }
}
