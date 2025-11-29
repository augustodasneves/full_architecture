using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using Shared.DTOs;
using Shared.Events;
using System.Text;

namespace PiiUpdateWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ServiceBusClient _serviceBusClient;
    private readonly ServiceBusProcessor _processor;
    private readonly HttpClient _httpClient;
    private readonly string _userApiUrl;

    public Worker(ILogger<Worker> logger, ServiceBusClient serviceBusClient, HttpClient httpClient, IConfiguration configuration)
    {
        _logger = logger;
        _serviceBusClient = serviceBusClient;
        _processor = _serviceBusClient.CreateProcessor("user-update-requests", new ServiceBusProcessorOptions());
        _httpClient = httpClient;
        _userApiUrl = configuration["UserAccountApi:BaseUrl"] ?? "http://user-account-api:8080";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _processor.ProcessMessageAsync += MessageHandler;
        _processor.ProcessErrorAsync += ErrorHandler;

        await _processor.StartProcessingAsync(stoppingToken);

        _logger.LogInformation("Worker started processing messages.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }

        await _processor.StopProcessingAsync(stoppingToken);
    }

    private async Task MessageHandler(ProcessMessageEventArgs args)
    {
        string body = args.Message.Body.ToString();
        _logger.LogInformation($"Received: {body}");

        try
        {
            var updateEvent = JsonConvert.DeserializeObject<UserUpdateRequestedEvent>(body);
            if (updateEvent != null)
            {
                var dto = new UserProfileDto
                {
                    PhoneNumber = updateEvent.NewPhoneNumber,
                    Email = updateEvent.NewEmail,
                    Address = updateEvent.NewAddress,
                    Name = "Updated User" // In real app, might need to fetch existing name or allow update
                };

                var json = JsonConvert.SerializeObject(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PutAsync($"{_userApiUrl}/api/user/update", content);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("User updated successfully via API.");
            }

            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message.");
            // Deadletter?
        }
    }

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Message handler encountered an exception");
        return Task.CompletedTask;
    }
}
