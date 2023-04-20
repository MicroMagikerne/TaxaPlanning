namespace PlanningService;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;
using PlanningService.Models;
using PlanningService.Services;


public class TopicWorker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private string CSVPath = string.Empty;
    private string RHQHN = string.Empty;

    public TopicWorker(ILogger<Worker> logger, IConfiguration configuration)
    {

        _logger = logger;

        //her brude vi laver flere logging kommandoer og tilføje dynamiske beskeder.
        //her burde vi også tænke over om der burde være flere env variabler, fx navnet på den queue vi prøver at consume

        CSVPath = configuration["CSVPath2"] ?? string.Empty;
        RHQHN = configuration["RHQHN"] ?? string.Empty;
        _logger.LogInformation($"path: {CSVPath}, HostName: {RHQHN}");
        
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //connection
        var factory = new ConnectionFactory { HostName = RHQHN };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        var hostName = System.Net.Dns.GetHostName();
        var ips = System.Net.Dns.GetHostAddresses(hostName);
        var _ipaddr = ips.First().MapToIPv4().ToString();
        _logger.LogInformation(1, $"Taxabooking responding from {_ipaddr}");
        
        _logger.LogInformation($"Nu er der lavet connection til:{connection.ToString}");

        _logger.LogInformation("Connection til rabbit etableret");
        CSVService service = new CSVService();

channel.ExchangeDeclare(exchange: "topic_logs", type: ExchangeType.Topic);
var queueName = channel.QueueDeclare().QueueName;

channel.QueueBind(queue: queueName,
                      exchange: "topic_logs",
                      routingKey: "public.repair");
channel.QueueBind(queue: queueName,
                      exchange: "topic_logs",
                      routingKey: "public.service");


        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Plan newplan = JsonSerializer.Deserialize<Plan>(message);

            if(newplan.RepairOrService == "repair")
            {
                service.AppendCSV2(CSVPath + "/repair.csv", newplan);
                _logger.LogInformation("Ny plan tilføjet, navnet på personen er:" + newplan.Model + "og path er: " + CSVPath + "repair.csv") ;
            }
            else
            {
                service.AppendCSV2(CSVPath + "/service.csv", newplan);
            }
            
        };

        channel.BasicConsume(queue: queueName,
                            autoAck: true,
                            consumer: consumer);
       

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(3000, stoppingToken);
        }
    }
}
