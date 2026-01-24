using AIChatService.Flow;
using AIChatService.Services;
using AIChatService.Validators;
using Azure.Messaging.ServiceBus;
using MongoDB.Driver;
using Shared.Interfaces;
using StackExchange.Redis;

namespace AIChatService.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Redis
        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis") ?? "localhost"));

        // MongoDB
        services.AddSingleton<IMongoClient>(sp =>
        {
            var connectionString = configuration["MongoDB:ConnectionString"];
            return new MongoClient(connectionString);
        });

        // Service Bus
        services.AddSingleton(sp => 
            new ServiceBusClient(configuration["ServiceBus:ConnectionString"]));

        // HTTP Clients
        services.AddHttpClient<ILLMService, LLMService>();

        services.AddHttpClient<IWhatsAppService, WhatsAppHttpService>(client =>
        {
            var whatsappProxyUrl = configuration["WhatsAppProxy:BaseUrl"] ?? "http://whatsapp-proxy-api:8080";
            client.BaseAddress = new Uri(whatsappProxyUrl);
        });

        services.AddHttpClient<IUserAccountService, UserAccountHttpService>(client =>
        {
            var userAccountUrl = configuration["UserAccountApi:BaseUrl"] ?? "http://user-account-api:8080";
            client.BaseAddress = new Uri(userAccountUrl);
        });

        // Validators
        services.AddSingleton<PhoneValidator>();
        services.AddSingleton<EmailValidator>();
        services.AddSingleton<AddressValidator>();

        // Infrastructure Services
        services.AddSingleton<DataAnonymizationService>();
        services.AddSingleton<FlowHistoryService>();

        // Domain Services
        services.AddScoped<ConversationService>();
        services.AddScoped<FlowEngine>();

        // Flow State Handlers (State Pattern)
        services.AddScoped<IFlowStateHandler, IdleStateHandler>();
        services.AddScoped<IFlowStateHandler, ConfirmingUpdateStateHandler>();
        services.AddScoped<IFlowStateHandler, CollectingPhoneStateHandler>();
        services.AddScoped<IFlowStateHandler, CollectingEmailStateHandler>();
        services.AddScoped<IFlowStateHandler, CollectingAddressStateHandler>();
        services.AddScoped<IFlowStateHandler, ConfirmingDataStateHandler>();

        return services;
    }
}
