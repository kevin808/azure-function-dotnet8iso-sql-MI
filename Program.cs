using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using FunctionApp.Models;
using FunctionApp.Helper;
using ALD_LogicProcessing.Middleware;
using Microsoft.Extensions.Azure;

namespace FunctionApp
{
    public class Program
    {
        public static async Task Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWebApplication(worker => worker.UseMiddleware<ExceptionHandlerMiddleware>())
                .ConfigureServices((context, services) =>
                {
                    services.AddDbContext<AppDbContext>(options =>
                        options.UseSqlServer(Environment.GetEnvironmentVariable("SqlConnectionString")));

                    services.AddSingleton<SqlHelper>();
                    services.AddLogging();

                    // Add AzureEventSourceLogForwarder
                    services.AddAzureClientsCore();
                })
                .Build();

            await host.RunAsync();
        }
    }
}