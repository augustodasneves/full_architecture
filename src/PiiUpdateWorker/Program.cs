using Azure.Messaging.ServiceBus;
using PiiUpdateWorker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton(sp => 
    new ServiceBusClient(builder.Configuration["ServiceBus:ConnectionString"]));

builder.Services.AddHttpClient();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
