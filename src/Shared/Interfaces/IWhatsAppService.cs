using Shared.DTOs;

namespace Shared.Interfaces;

public interface IWhatsAppService
{
    Task<WhatsAppMessageResponse> SendMessageAsync(string to, string message);
}
