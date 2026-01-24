namespace WhatsAppProxyApi.Models;

public class WhatsAppSettings
{
    public string BaileysServiceUrl { get; set; } = "http://baileys-whatsapp-service:3000";
    public string VerifyToken { get; set; } = string.Empty;
    public string AppSecret { get; set; } = string.Empty;
}
