using MBKC.Service.Services.Implementations;
using MBKC.Service.Services.Interfaces;
using MBKC.WokerService.Extentions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Serilog;
using System.Net.Http.Headers;
using System.Text;

namespace MBKC.WokerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                       .ConfigureServices(services =>
                       {
                           services.AddHostedService<Worker>();
                           services.AddUnitOfWork();
                           services.AddServices();
                           Log.Logger = new LoggerConfiguration()
                                            .MinimumLevel.Information()
                                            .WriteTo.Console()
                                            .WriteTo.File("logs/mbkcBotLog-.txt", rollingInterval: RollingInterval.Day)
                                            .CreateLogger();
                       }).UseSerilog();
        }
    }
}