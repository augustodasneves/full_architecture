using Newtonsoft.Json;
using Shared.Interfaces;
using System.Text;

namespace AIChatService.Services;

public class LLMService : ILLMService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public LLMService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _baseUrl = configuration["LLM:BaseUrl"] ?? "http://localhost:11434";
    }

    public async Task<string> IdentifyIntentAsync(string userMessage)
    {
        var prompt = $"Analyze the following message and determine if the user wants to update their registration data. Reply with only 'UPDATE_REGISTRATION' or 'OTHER'. Message: \"{userMessage}\"";
        return await CallOllamaAsync(prompt);
    }

    public async Task<string> ExtractEntityAsync(string userMessage, string entityType)
    {
        var prompt = $"Extract the {entityType} from the following message. Reply with only the extracted value, or 'NOT_FOUND' if not present. Message: \"{userMessage}\"";
        return await CallOllamaAsync(prompt);
    }

    private async Task<string> CallOllamaAsync(string prompt)
    {
        var request = new
        {
            model = "llama3",
            prompt = prompt,
            stream = false
        };

        var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
        try
        {
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/generate", content);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject(responseString)!;
            return result.response.ToString().Trim();
        }
        catch (Exception)
        {
            // Fallback for demo or if LLM is down
            return "OTHER"; 
        }
    }
}
