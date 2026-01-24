using Shared.DTOs;

namespace WhatsAppProxyApi.Services;

public interface IWhatsAppService
{
    Task<WhatsAppMessageResponse> SendTextMessageAsync(string to, string message);
    Task<(bool Success, string QrCode, string Message)> GetQrCodeAsync();
    Task<(bool Connected, string State)> GetConnectionStatusAsync();
}
