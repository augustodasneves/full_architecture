using Shared.Interfaces;
using System.Net;
using Newtonsoft.Json;

namespace AIChatService.Services;

public class UserAccountHttpService : IUserAccountService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserAccountHttpService> _logger;

    public UserAccountHttpService(HttpClient httpClient, ILogger<UserAccountHttpService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> UserExistsAsync(string phoneNumber)
    {
        try
        {
            var cleanNumber = ExtractNumber(phoneNumber);
            var url = $"/api/User/me/{cleanNumber}";
            
            _logger.LogInformation("Checking user existence. Original: {Raw}, Extracted: {Clean}. Calling: {Url}", 
                phoneNumber, cleanNumber, url);
                
            var response = await _httpClient.GetAsync(url);
            _logger.LogInformation("User check response for {Target}: {Status}", cleanNumber, response.StatusCode);
            
            return response.StatusCode == HttpStatusCode.OK;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user exists: {PhoneNumber}", phoneNumber);
            return false;
        }
    }

    public async Task<Shared.DTOs.UserProfileDto?> GetUserProfileAsync(string phoneNumber)
    {
        try
        {
            var cleanNumber = ExtractNumber(phoneNumber);
            var url = $"/api/User/me/{cleanNumber}";
            
            _logger.LogInformation("Fetching user profile. Original: {Raw}, Extracted: {Clean}", 
                phoneNumber, cleanNumber);
                
            var response = await _httpClient.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                var profile = JsonConvert.DeserializeObject<Shared.DTOs.UserProfileDto>(content);
                _logger.LogInformation("Profile found for {Clean}: {Name}", cleanNumber, profile?.Name);
                return profile;
            }
            
            _logger.LogInformation("No profile found for {Clean} (Status: {Status})", cleanNumber, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile: {PhoneNumber}", phoneNumber);
            return null;
        }
    }

    private string ExtractNumber(string jid)
    {
        if (string.IsNullOrEmpty(jid)) return string.Empty;
        
        // Se contiver @, pega apenas a parte da frente (o número ou id)
        // Ex: 555198801001@s.whatsapp.net -> 555198801001
        // Ex: 216947488772305@lid -> 216947488772305
        return jid.Split('@')[0];
    }
}
