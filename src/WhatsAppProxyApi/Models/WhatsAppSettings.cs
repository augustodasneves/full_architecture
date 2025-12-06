namespace WhatsAppProxyApi.Models;

public class WhatsAppSettings
{
    public string AccessToken { get; set; } = string.Empty;
    public string PhoneNumberId { get; set; } = string.Empty;
    public string ApiVersion { get; set; } = "v21.0";
    public string BaseUrl { get; set; } = "https://graph.facebook.com";
    public string VerifyToken { get; set; } = string.Empty;
}
