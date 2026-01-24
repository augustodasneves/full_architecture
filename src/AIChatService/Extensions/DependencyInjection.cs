using AIChatService.Flow;
using AIChatService.Services;
using AIChatService.Validators;
using Azure.Messaging.ServiceBus;
using MongoDB.Driver;
using Shared.Interfaces;
using AIChatService.Intents;
using StackExchange.Redis;

namespace AIChatService.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(typeof(DependencyInjection));

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
        }).AddStandardResilienceHandler(options =>
        {
            options.Retry.MaxRetryAttempts = 3;
            options.Retry.Delay = TimeSpan.FromSeconds(2);
            options.CircuitBreaker.FailureRatio = 0.5;
            options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30);
        });

        services.AddHttpClient<IUserAccountService, UserAccountHttpService>(client =>
        {
            var userAccountUrl = configuration["UserAccountApi:BaseUrl"] ?? "http://user-account-api:8080";
            client.BaseAddress = new Uri(userAccountUrl);
        }).AddStandardResilienceHandler(options =>
        {
            options.Retry.MaxRetryAttempts = 2;
            options.Retry.Delay = TimeSpan.FromSeconds(1);
            options.CircuitBreaker.FailureRatio = 0.5;
            options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(15);
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
        services.AddScoped<IFlowStateHandler, CollectingNameStateHandler>();
        services.AddScoped<IFlowStateHandler, CollectingPhoneStateHandler>();
        services.AddScoped<IFlowStateHandler, CollectingEmailStateHandler>();
        services.AddScoped<IFlowStateHandler, CollectingAddressStateHandler>();
        services.AddScoped<IFlowStateHandler, ConfirmingDataStateHandler>();

        // Intent Strategies
        services.AddScoped<IIntentStrategy, UpdateRegistrationStrategy>();
        services.AddScoped<IIntentStrategy, OtherIntentStrategy>();

        // Background Consumers
        services.AddHostedService<WhatsAppMessageConsumer>();

        // Health Checks
        services.AddHealthChecks()
            .AddRedis(configuration.GetConnectionString("Redis") ?? "localhost", name: "redis")
            .AddMongoDb(sp => sp.GetRequiredService<IMongoClient>(), name: "mongodb")
            .AddAzureServiceBusQueue(configuration["ServiceBus:ConnectionString"]!, "whatsapp-messages", name: "servicebus");

        return services;
    }
}
