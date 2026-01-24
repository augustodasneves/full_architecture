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
            // 1. Try search by full WhatsApp JID first (more reliable)
            var waUrl = $"/api/User/wa/{phoneNumber}";
            _logger.LogInformation("Checking user existence by WA ID. Calling: {Url}", waUrl);
            var waResponse = await _httpClient.GetAsync(waUrl);
            
            if (waResponse.StatusCode == HttpStatusCode.OK)
            {
                _logger.LogInformation("User found by WhatsApp ID: {WhatsAppId}", phoneNumber);
                return true;
            }

            // 2. Fallback to cleaned phone number search
            var cleanNumber = ExtractNumber(phoneNumber);
            var phoneUrl = $"/api/User/me/{cleanNumber}";
            
            _logger.LogInformation("User not found by JID. Trying cleaned phone number: {Clean}. Calling: {Url}", 
                cleanNumber, phoneUrl);
                
            var phoneResponse = await _httpClient.GetAsync(phoneUrl);
            _logger.LogInformation("User check response for {Target}: {Status}", cleanNumber, phoneResponse.StatusCode);
            
            return phoneResponse.StatusCode == HttpStatusCode.OK;
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
            // 1. Try search by full WhatsApp JID first
            var waUrl = $"/api/User/wa/{phoneNumber}";
            _logger.LogInformation("Fetching profile by WA ID. Calling: {Url}", waUrl);
            var waResponse = await _httpClient.GetAsync(waUrl);
            
            if (waResponse.StatusCode == HttpStatusCode.OK)
            {
                var content = await waResponse.Content.ReadAsStringAsync();
                var profile = JsonConvert.DeserializeObject<Shared.DTOs.UserProfileDto>(content);
                _logger.LogInformation("Profile found by WhatsApp ID {Raw}: {Name}", phoneNumber, profile?.Name);
                return profile;
            }

            // 2. Fallback to cleaned phone number search
            var cleanNumber = ExtractNumber(phoneNumber);
            var phoneUrl = $"/api/User/me/{cleanNumber}";
            
            _logger.LogInformation("Profile not found by JID. Trying cleaned phone number: {Clean}. Calling: {Url}", 
                cleanNumber, phoneUrl);
                
            var phoneResponse = await _httpClient.GetAsync(phoneUrl);
            if (phoneResponse.StatusCode == HttpStatusCode.OK)
            {
                var content = await phoneResponse.Content.ReadAsStringAsync();
                var profile = JsonConvert.DeserializeObject<Shared.DTOs.UserProfileDto>(content);
                _logger.LogInformation("Profile found by phone {Clean}: {Name}", cleanNumber, profile?.Name);
                return profile;
            }
            
            _logger.LogInformation("No profile found for {Raw} or {Clean}", phoneNumber, cleanNumber);
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

    public async Task<Shared.DTOs.UserProfileDto?> GetUserProfileByWhatsAppIdAsync(string whatsappId)
    {
        try
        {
            var waUrl = $"/api/User/wa/{whatsappId}";
            _logger.LogInformation("Explicitly fetching profile by WA ID. Calling: {Url}", waUrl);
            var waResponse = await _httpClient.GetAsync(waUrl);
            
            if (waResponse.StatusCode == HttpStatusCode.OK)
            {
                var content = await waResponse.Content.ReadAsStringAsync();
                var profile = JsonConvert.DeserializeObject<Shared.DTOs.UserProfileDto>(content);
                _logger.LogInformation("Profile found for WA ID {Raw}: {Name}", whatsappId, profile?.Name);
                return profile;
            }

            _logger.LogInformation("No profile found for WA ID {Raw}", whatsappId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile by WA ID: {WhatsAppId}", whatsappId);
            return null;
        }
    }
}
