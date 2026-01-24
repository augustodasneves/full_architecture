using AIChatService.Services;
using Azure.Messaging.ServiceBus;
using Shared.Events;
using System.Text.Json;

namespace AIChatService.Services;

public class WhatsAppMessageConsumer : BackgroundService
{
    private readonly ServiceBusClient _serviceBusClient;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WhatsAppMessageConsumer> _logger;
    private ServiceBusProcessor? _processor;

    public WhatsAppMessageConsumer(
        ServiceBusClient serviceBusClient,
        IServiceProvider serviceProvider,
        ILogger<WhatsAppMessageConsumer> logger)
    {
        _serviceBusClient = serviceBusClient;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _processor = _serviceBusClient.CreateProcessor("whatsapp-messages", new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,
            MaxConcurrentCalls = 10 // Control concurrency here
        });

        _processor.ProcessMessageAsync += ProcessMessageAsync;
        _processor.ProcessErrorAsync += ProcessErrorAsync;

        _logger.LogInformation("Starting WhatsApp message consumer...");
        await _processor.StartProcessingAsync(stoppingToken);

        // Keep the task running until stoppingToken is canceled
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        try
        {
            var body = args.Message.Body.ToString();
            var messageData = JsonSerializer.Deserialize<WhatsAppMessageReceivedEvent>(body);

            if (messageData != null)
            {
                _logger.LogInformation("Processing message from {From}: {Content}", messageData.From, messageData.Content);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var flowEngine = scope.ServiceProvider.GetRequiredService<FlowEngine>();
                    await flowEngine.ProcessMessageAsync(messageData.From, messageData.Content);
                }
            }

            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Service Bus message");
            // Optionally dead-letter the message or just let it time out for retry
        }
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Service Bus processor error: {Source}", args.ErrorSource);
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_processor != null)
        {
            await _processor.StopProcessingAsync(cancellationToken);
            await _processor.DisposeAsync();
        }
        await base.StopAsync(cancellationToken);
    }
}
