using AIChatService.Models;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace AIChatService.Services;

public class ConversationService
{
    private readonly IConnectionMultiplexer _redis;

    public ConversationService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<ConversationState> GetStateAsync(string phoneNumber)
    {
        var db = _redis.GetDatabase();
        var json = await db.StringGetAsync($"conversation:{phoneNumber}");
        if (json.IsNullOrEmpty)
        {
            return new ConversationState { PhoneNumber = phoneNumber };
        }
        return JsonConvert.DeserializeObject<ConversationState>(json!)!;
    }

    public async Task SaveStateAsync(ConversationState state)
    {
        var db = _redis.GetDatabase();
        var json = JsonConvert.SerializeObject(state);
        await db.StringSetAsync($"conversation:{state.PhoneNumber}", json, TimeSpan.FromMinutes(30));
    }
}
