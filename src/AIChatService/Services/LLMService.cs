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
        var prompt = $@"Analyze the following message and determine the user's intent:
- If the user wants to update their registration data, initiate a new process, start the service flow, or change their information, reply with 'UPDATE_REGISTRATION'.
- If the user is just saying hello, asking a general question, or anything else that doesn't involve starting a registration/update process, reply with 'OTHER'.

Common keywords in Portuguese that indicate 'UPDATE_REGISTRATION': 'iniciar', 'começar', 'atualizar', 'mudar', 'alterar', 'cadastro'.

Message: ""{userMessage}""
Reply with only 'UPDATE_REGISTRATION' or 'OTHER'.";

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
            model = "phi3",
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
