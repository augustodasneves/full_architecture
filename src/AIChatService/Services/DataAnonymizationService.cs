using System.Security.Cryptography;
using System.Text;

namespace AIChatService.Services;

public class DataAnonymizationService
{
    private readonly IConfiguration _configuration;
    private readonly string _saltKey;
    
    public DataAnonymizationService(IConfiguration configuration)
    {
        _configuration = configuration;
        _saltKey = _configuration["Anonymization:SaltKey"] ?? "default-salt-key-change-in-production";
    }
    
    // Hash irreversível usando SHA256 + salt
    public string HashData(string data)
    {
        using var sha256 = SHA256.Create();
        var saltedData = data + _saltKey;
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedData));
        return Convert.ToBase64String(bytes);
    }
    
    // Mascaramento de números de telefone
    public string MaskPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber) || phoneNumber.Length < 4)
            return "****";
        
        var visible = phoneNumber.Substring(phoneNumber.Length - 4);
        var masked = new string('*', phoneNumber.Length - 4);
        return masked + visible;
    }
    
    // Mascaramento de email
    public string MaskEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            return "****@****.***";
        
        var parts = email.Split('@');
        var localPart = parts[0];
        var domain = parts[1];
        
        var visibleLocal = localPart.Length > 2 ? localPart.Substring(0, 2) : localPart;
        var maskedLocal = visibleLocal + new string('*', Math.Max(0, localPart.Length - 2));
        
        return $"{maskedLocal}@{domain}";
    }
    
    // Gera ID único de fluxo de atendimento
    public string GenerateFlowId(string phoneNumber)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var random = Guid.NewGuid().ToString("N").Substring(0, 8);
        var combined = $"{phoneNumber}_{timestamp}_{random}";
        return HashData(combined).Substring(0, 20); // ID único
    }
}
