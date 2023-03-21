namespace PlanningService;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;
using PlanningService.Models;
using PlanningService.Services;


public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private string CSVPath = string.Empty;
    private string RHQHN = string.Empty;

    public Worker(ILogger<Worker> logger, IConfiguration configuration)
    {

        _logger = logger;

        //her brude vi laver flere logging kommandoer og tilføje dynamiske beskeder.
        //her burde vi også tænke over om der burde være flere env variabler, fx navnet på den queue vi prøver at consume

        CSVPath = configuration["CSVPath"] ?? string.Empty;
        RHQHN = configuration["RMQHN"] ?? string.Empty;
        _logger.LogInformation($"path: {CSVPath}, HostName: {RHQHN}");
        
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //connection
        var factory = new ConnectionFactory { HostName = RHQHN };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        _logger.LogInformation($"Nu er der lavet connection til:{connection.ToString}");

        _logger.LogInformation("Connection til rabbit etableret");
        CSVService service = new CSVService();

        //queue declaration
        channel.QueueDeclare(queue: "booking_queue",
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

        _logger.LogInformation("connected til booking_queue");

        //hvad sker der hvis consumer får nok guf:
        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            PlanDTO newplan = JsonSerializer.Deserialize<PlanDTO>(message);
            service.AppendCSV(CSVPath, newplan);
            _logger.LogInformation("Ny plan tilføjet, navnet på personen er:" + newplan.Kundenavn);
        };
        //basicConsume
        channel.BasicConsume(queue: "booking_queue",
                            autoAck: true,
                            consumer: consumer);
       

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(3000, stoppingToken);
        }
    }
}
