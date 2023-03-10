using PlanningService;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using PlanningService.Models;
using PlanningService.Services;
using System.Text.Json;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();
