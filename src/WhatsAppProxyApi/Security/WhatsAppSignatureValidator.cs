using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using WhatsAppProxyApi.Models;

namespace WhatsAppProxyApi.Security;

public class WhatsAppSignatureValidator
{
    private readonly WhatsAppSettings _settings;
    private readonly ILogger<WhatsAppSignatureValidator> _logger;

    public WhatsAppSignatureValidator(IOptions<WhatsAppSettings> settings, ILogger<WhatsAppSignatureValidator> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<bool> IsValid(HttpRequest request)
    {
        if (string.IsNullOrEmpty(_settings.AppSecret))
        {
            _logger.LogWarning("AppSecret is not configured. Webhook validation is disabled but security is compromised.");
            return true; // Or false depending on strictness. Let's return true for now but log.
        }

        if (!request.Headers.TryGetValue("X-Hub-Signature-256", out var signatureHeader))
        {
            _logger.LogWarning("X-Hub-Signature-256 header is missing.");
            return false;
        }

        var signature = signatureHeader.ToString();
        if (signature.StartsWith("sha256="))
        {
            signature = signature.Substring(7);
        }

        request.EnableBuffering();
        using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;

        var keyBytes = Encoding.UTF8.GetBytes(_settings.AppSecret);
        var bodyBytes = Encoding.UTF8.GetBytes(body);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(bodyBytes);
        var computedSignature = Convert.ToHexString(hashBytes).ToLower();

        if (computedSignature != signature.ToLower())
        {
            _logger.LogWarning("Invalid signature. Computed: {Computed}, Received: {Received}", computedSignature, signature);
            return false;
        }

        return true;
    }
}
