using System.Text.Json;
using Azure.Messaging.ServiceBus;
using SupportAgent.Application.Services;

namespace SupportAgent.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private ServiceBusClient _client;
        private ServiceBusProcessor _processor;

        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var connectionString = _configuration.GetConnectionString("ServiceBus");
            var queueName = _configuration["ServiceBus:QueueName"] ?? "user-pii-update-queue";

            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogWarning("Service Bus connection string not found. Worker will not consume messages.");
                return;
            }

            _client = new ServiceBusClient(connectionString);
            _processor = _client.CreateProcessor(queueName, new ServiceBusProcessorOptions());

            _processor.ProcessMessageAsync += MessageHandler;
            _processor.ProcessErrorAsync += ErrorHandler;

            await _processor.StartProcessingAsync(stoppingToken);

            _logger.LogInformation("Worker started processing messages.");

            // Wait until stopped
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }

            await _processor.StopProcessingAsync(stoppingToken);
            await _processor.DisposeAsync();
            await _client.DisposeAsync();
        }

        private async Task MessageHandler(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();
            _logger.LogInformation($"Received message: {body}");

            try
            {
                var updateData = JsonSerializer.Deserialize<PiiUpdateMessage>(body);
                if (updateData != null)
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var handler = scope.ServiceProvider.GetRequiredService<IPiiUpdateHandler>();
                        await handler.UpdatePiiAsync(updateData.UserId, updateData.PhoneNumber, updateData.Address);
                    }
                }

                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message.");
                // Retry policy is handled by Service Bus default settings (MaxDeliveryCount)
                // But we can also abandon to retry immediately
                await args.AbandonMessageAsync(args.Message);
            }
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception, "Message handler encountered an exception");
            return Task.CompletedTask;
        }
    }

    public class PiiUpdateMessage
    {
        public Guid UserId { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
    }
}
