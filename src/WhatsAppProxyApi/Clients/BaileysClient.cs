using Microsoft.Extensions.Options;
using Shared.DTOs;
using System.Text;
using System.Text.Json;
using WhatsAppProxyApi.Models;

namespace WhatsAppProxyApi.Clients;

public class BaileysClient : IBaileysClient
{
    private readonly HttpClient _httpClient;
    private readonly WhatsAppSettings _settings;
    private readonly ILogger<BaileysClient> _logger;

    public BaileysClient(HttpClient httpClient, IOptions<WhatsAppSettings> settings, ILogger<BaileysClient> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<WhatsAppMessageResponse> SendMessageAsync(string to, string message)
    {
        try
        {
            var url = $"{_settings.BaileysServiceUrl}/send";
            var payload = new { to, message };
            var jsonContent = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            _logger.LogInformation("Sending WhatsApp message to {To} via Baileys", to);
            
            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var messageId = result.GetProperty("messageId").GetString();
                return new WhatsAppMessageResponse { Success = true, MessageId = messageId };
            }

            _logger.LogError("Failed to send message. Status: {Status}, Response: {Response}", response.StatusCode, responseContent);
            return new WhatsAppMessageResponse { Success = false, Error = $"Baileys API error: {response.StatusCode}" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while sending WhatsApp message to {To}", to);
            return new WhatsAppMessageResponse { Success = false, Error = ex.Message };
        }
    }

    public async Task<(bool Success, string QrCode, string Message)> GetQrCodeAsync()
    {
        try
        {
            var url = $"{_settings.BaileysServiceUrl}/qr";
            var response = await _httpClient.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var success = result.GetProperty("success").GetBoolean();
                var qr = success ? result.GetProperty("qr").GetString() ?? string.Empty : string.Empty;
                var message = result.GetProperty("message").GetString() ?? string.Empty;
                return (success, qr, message);
            }
            return (false, string.Empty, "Failed to get QR code");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while getting QR code");
            return (false, string.Empty, ex.Message);
        }
    }

    public async Task<(bool Connected, string State)> GetConnectionStatusAsync()
    {
        try
        {
            var url = $"{_settings.BaileysServiceUrl}/status";
            var response = await _httpClient.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var connected = result.GetProperty("connected").GetBoolean();
                var state = result.GetProperty("state").GetString() ?? "unknown";
                return (connected, state);
            }
            return (false, "unknown");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while getting connection status");
            return (false, "error");
        }
    }
}
