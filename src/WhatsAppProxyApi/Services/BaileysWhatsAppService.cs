using Shared.DTOs;
using WhatsAppProxyApi.Clients;

namespace WhatsAppProxyApi.Services;

public class BaileysWhatsAppService : IWhatsAppService
{
    private readonly IBaileysClient _baileysClient;
    private readonly ILogger<BaileysWhatsAppService> _logger;

    public BaileysWhatsAppService(IBaileysClient baileysClient, ILogger<BaileysWhatsAppService> logger)
    {
        _baileysClient = baileysClient;
        _logger = logger;
    }

    public async Task<WhatsAppMessageResponse> SendTextMessageAsync(string to, string message)
    {
        _logger.LogInformation("Processing send message request to {To}", to);
        return await _baileysClient.SendMessageAsync(to, message);
    }

    public async Task<(bool Success, string QrCode, string Message)> GetQrCodeAsync()
    {
        return await _baileysClient.GetQrCodeAsync();
    }

    public async Task<(bool Connected, string State)> GetConnectionStatusAsync()
    {
        return await _baileysClient.GetConnectionStatusAsync();
    }
}
