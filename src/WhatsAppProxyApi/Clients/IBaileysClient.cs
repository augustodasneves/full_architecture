using Shared.DTOs;

namespace WhatsAppProxyApi.Clients;

public interface IBaileysClient
{
    Task<WhatsAppMessageResponse> SendMessageAsync(string to, string message);
    Task<(bool Success, string QrCode, string Message)> GetQrCodeAsync();
    Task<(bool Connected, string State)> GetConnectionStatusAsync();
}
