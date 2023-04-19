using PlanningService;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using PlanningService.Models;
using PlanningService.Services;
using System.Text.Json;
using NLog;
using NLog.Web;

var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("init main");
try {
IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
    })
    
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        
    }).UseNLog()
    .Build();
logger.Info("jaja");
host.Run();
}
catch(Exception ex){
    logger.Error(ex, "Stopped program because of exception");
throw;
}
finally
{
     NLog.LogManager.Shutdown();
}